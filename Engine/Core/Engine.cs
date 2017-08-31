using HtmlAgilityPack;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using static Engine.Contracts;

namespace Engine.Core
{
    public static class LSA
    {
        public static string DictionaryPath = "D:/Wiki/dict.txt";

        public static MatrixContainer MatrixContainer { get; set; }
        private static Dictionary<int, MatrixContainer> _matrixContainers { get; set; } = new Dictionary<int, MatrixContainer>();

        public static readonly HashSet<string> Exclusions = new HashSet<string>() { "me", "you", "his", "him", "her", "herself", "no", "gnu", "disclaimers", "copyrights", "navigation", "donate", "documentation", "trademark", "revision", "contact", "modified", "charity", "registered", "portal", "views", "free", "recent", "search", "details", "license", "encyclopedia", "page", "terms", "current", "content", "foundation", "categories", "help", "changes", "discussion", "users", "featured", "article" };

        public static void CreateDocument(Stream blobStream, string documentName)
        {
            using (var context = new SvdEntities())
            {
                var document = context.Documents.FirstOrDefault(d => d.Name == documentName);

                if (document == null)
                {
                    var termLookup = GetOrAddTerms(context).ToLookup(t => t.Value);
                    var sr = new StreamReader(blobStream);
                    var html = sr.ReadToEnd();

                    document = context.Documents.Add(new Document()
                    {
                        Name = documentName.Trim('"')
                    });

                    HtmlDocument doc = new HtmlDocument();

                    doc.LoadHtml(HttpUtility.HtmlDecode(html));

                    var termDocCounts = new List<TermDocumentCount>();

                    doc.DocumentNode.SelectNodes("//body//text()").ToList().ForEach(node =>
                    {
                        var text = node.InnerText.Trim();

                        if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                        {
                            var chars = text.Where(c => (
                                char.IsLetterOrDigit(c) ||
                                char.IsWhiteSpace(c) ||
                                c == '-'))
                                .ToArray();

                            text = new string(chars);

                            foreach (var _token in text.Trim().Split(' '))
                            {
                                var miniToken = _token.Trim().ToLower();

                                var termList = termLookup[miniToken].ToList();

                                if (!string.IsNullOrEmpty(miniToken) && miniToken != "-" && miniToken != "\n" && termList.Count > 0)
                                {
                                    termDocCounts.Add(new TermDocumentCount()
                                    {
                                        Document = document,
                                        Term = termList.First()
                                    });
                                }
                            }
                        }
                    });

                    var newTdc = from tdc in termDocCounts
                                 group tdc by new
                                 {
                                     DocumentId = tdc.Document.Id,
                                     TermId = tdc.Term.Id
                                 } into g
                                 let tdc = g.First()
                                 select new TermDocumentCount()
                                 {
                                     Document = tdc.Document,
                                     Term = tdc.Term,
                                     DocumentId = g.Key.DocumentId,
                                     TermId = g.Key.TermId,
                                     Count = g.Count()
                                 };


                    context.TermDocumentCounts.AddRange(newTdc);
                    context.SaveChanges();
                }
            }
        }

        public static IEnumerable<Term> GetOrAddTerms(SvdEntities context)
        {
            var terms = context.Terms.ToList();

            if (!terms.Any()) {
                var fileStreamDict = new StreamReader(DictionaryPath);

                terms = new List<Term>();

                while (!fileStreamDict.EndOfStream)
                {
                    terms.Add(new Term()
                    {
                        Value = fileStreamDict.ReadLine()
                    });
                }

                terms = context.Terms.AddRange(terms).ToList();

                context.SaveChanges();
            }

            return terms;
        }

        public static DenseMatrix GetTermDocMatrix(SvdEntities context, Job job, IEnumerable<int> docIds)
        {
            var termLookup = GetOrAddTerms(context).ToLookup(t => t.Value);
            
            SetJobStatus(context, job, JobStatus.BuildingMatrix);

            var readFilesStart = DateTime.Now;

            var _docIds = docIds.ToArray();
            var files = context.Documents.Where(d => _docIds.Contains(d.Id)).Select(d => d.Name).ToList();

            var newDocuments = new List<Document>();
            var jobDocuments = new List<JobDocument>();
            var termDocCounts = new List<TermDocumentCount>();
            var documentLookup = context.Documents.ToLookup(d => d.Name);

            // Create Documents
            foreach (var file in files)
            {
                var docEntity = documentLookup[file].FirstOrDefault();

                if(docEntity == null)
                {
                    docEntity = new Document()
                    {
                        Name = file
                    };

                    newDocuments.Add(docEntity);
                }
                else
                {
                    termDocCounts.AddRange(docEntity.TermDocumentCounts);
                }

                jobDocuments.Add(new JobDocument()
                {
                    Job = job,
                    Document = docEntity, 
                    OrdinalIndex = files.IndexOf(file)
                });
            } 

            context.Documents.AddRange(newDocuments);
            context.JobDocuments.AddRange(jobDocuments);

            context.SaveChanges();

            // Setup Parallel Collections
            
            ConcurrentBag<TermDocumentCount> termDocCountsBagCalculated = new ConcurrentBag<TermDocumentCount>();

            jobDocuments.AsParallel().ForAll((jobDocumentEntity) =>
            {
                if (jobDocumentEntity.Document.TermDocumentCounts.Count == 0)
                {
                    var html = File.ReadAllText(jobDocumentEntity.Document.Name, Encoding.UTF8);

                    HtmlDocument doc = new HtmlDocument();

                    doc.LoadHtml(HttpUtility.HtmlDecode(html));

                    doc.DocumentNode.SelectNodes("//body//text()").ToList().ForEach(node =>
                    {
                        var text = node.InnerText.Trim();

                        if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                        {
                            var chars = text.Where(c => (
                                char.IsLetterOrDigit(c) ||
                                char.IsWhiteSpace(c) ||
                                c == '-'))
                                .ToArray();

                            text = new string(chars);

                            ParseDocumentData(text, jobDocumentEntity.Document, termDocCountsBagCalculated, termLookup);
                        }
                    });
                }
            });

            // Build New Term/Doc Count Entites

            var newTdc = from tdc in termDocCountsBagCalculated
                            group tdc by new
                            {
                                DocumentId = tdc.Document.Id,
                                TermId = tdc.Term.Id
                            } into g
                        let tdc = g.First()
                        select new TermDocumentCount() {
                            Document = tdc.Document,
                            Term = tdc.Term,
                            DocumentId = g.Key.DocumentId,
                            TermId = g.Key.TermId,
                            Count = g.Count()
                        };

            context.TermDocumentCounts.AddRange(newTdc);
            termDocCounts.AddRange(newTdc);

            // Remove Exclusions from saved list
            termDocCounts = termDocCounts.Where(tdc => !Exclusions.Contains(tdc.Term.Value)).ToList();

            // Save Job Terms

            var termsList = termDocCounts.Select(tdc => tdc.Term.Value).Distinct().ToList();

            var jobTerms = from t in termsList
                           let termEntity = termLookup[t].First()
                           select new JobTerm()
                           {
                               Job = job,
                               TermId = termEntity.Id,
                               OrdinalIndex = termsList.IndexOf(t)
                           };

            context.JobTerms.AddRange(jobTerms);
            
            // Build Final Term/Doc Matrix

            var matrix = new DenseMatrix(termsList.Count, _docIds.Length);

            foreach (var termDocCount in termDocCounts)
            {
                matrix[termsList.IndexOf(termDocCount.Term.Value), files.IndexOf(termDocCount.Document.Name)] = termDocCount.Count;
            }

            Debug.WriteLine($"Read File Calc Time: {DateTime.Now.Subtract(readFilesStart).TotalMilliseconds} Milliseconds");

            return matrix;
        }

        public static void ParseDocumentData(string rawText, Document document, ConcurrentBag<TermDocumentCount> termDocCounts, ILookup<string,Term> termsLookup)
        {
            foreach (var _token in rawText.Trim().Split(' '))
            {
                var miniToken = _token.Trim().ToLower();

                var termList = termsLookup[miniToken].ToList();

                if (!string.IsNullOrEmpty(miniToken) && miniToken != "-" && miniToken != "\n" && termList.Count > 0)
                {
                    termDocCounts.Add(new TermDocumentCount()
                    {
                        Document = document,
                        Term = termList.First()
                    });
                }
            }
        }

        public static Svd<float> GetSvd(SvdEntities context, Job job, DenseMatrix termDocMatrix)
        {
            var svdStart = DateTime.Now;

            SetJobStatus(context, job, JobStatus.Svd);

            var svd = termDocMatrix.Svd();

            Debug.WriteLine($"SVD Calc Time: {DateTime.Now.Subtract(svdStart).TotalMilliseconds} Milliseconds");

            return svd;
        }

        public static void SetJobStatus(SvdEntities context, Job job, JobStatus status)
        {
            job.Status = status;
            context.SaveChanges();
        }

        public static void ProcessAndStore(int jobId, IEnumerable<int> docIds)
        {
            using (var context = new SvdEntities())
            {
                Job job = null;
                var _docIds = docIds.ToArray();

                try
                {
                    job = context.Jobs.Find(jobId);

                    // Process
                    var matrix = GetTermDocMatrix(context, job, _docIds);
                    var svd = GetSvd(context, job, matrix);

                    var dimensions = svd.S.Count <= 300 ? svd.S.Count : 300;
                     
                    var binaryFormatter = new BinaryFormatter();

                    // Reduction Step - U Table

                    var newUMatrix = new DenseMatrix(matrix.RowCount, dimensions);

                    for (var i = 0; i < dimensions; i++)
                    {
                        for (var m = 0; m < matrix.RowCount; m++)
                        {
                            newUMatrix[m, i] = svd.U[m, i] * svd.S[i];
                        }
                    }

                    using (var memoryStreamU = new MemoryStream())
                    {
                        binaryFormatter.Serialize(memoryStreamU, newUMatrix.Values);

                        memoryStreamU.Position = 0;

                        context.UMatrices.Add(new UMatrix()
                        {
                            Job = job,
                            SerializedValues = memoryStreamU.ToArray()
                        });
                    }

                    // Reduction Step - V Table

                    var newVMatrix = new DenseMatrix(dimensions, _docIds.Length);

                    for (var i = 0; i < dimensions; i++)
                    {
                        for (var m = 0; m < _docIds.Length; m++)
                        {
                            newVMatrix[i, m] = svd.VT[i, m] * svd.S[i];
                        }
                    }

                    using (var memoryStreamV = new MemoryStream())
                    {
                        binaryFormatter.Serialize(memoryStreamV, newVMatrix.Values);

                        memoryStreamV.Position = 0;

                        context.VMatrices.Add(new VMatrix()
                        {
                            Job = job,
                            SerializedValues = memoryStreamV.ToArray()
                        });
                    }

                    job.Dimensions = dimensions;
                    job.Completed = DateTime.Now;
                    job.Status = JobStatus.Completed;

                    context.SaveChanges();
                }
                catch (Exception)
                {
                    job.Status = JobStatus.Failed;
                    job.Completed = DateTime.Now;
                    context.SaveChanges();

                    throw;
                }
            }
        }

        public static void RunQueryFromFile(int jobId)
        {
            GetMatrixContainer(jobId);

            string query = null;

            while(query != "") {
                query = Console.ReadLine();

                //// Locate Query index in U Table

                var splitQuery = query.Split(' ');
                var totalQueryVector = new DenseVector(MatrixContainer.Dimensions);

                foreach (var miniQuery in splitQuery)
                {
                    var qI = MatrixContainer.Terms.ToList().IndexOf(miniQuery);

                    if(qI >= 0)
                    {
                        var qV = MatrixContainer.UMatrix.Row(qI) as DenseVector;

                        totalQueryVector += qV;
                    }
                }

                var queryVector = (totalQueryVector / splitQuery.Length).ToArray();

                Console.WriteLine("Query: " + query);
                Console.WriteLine("Query Vector: " + queryVector.ToString());
                Console.WriteLine("Dimensions: " + MatrixContainer.Dimensions);

                var docResults = new List<DocResult>();

                for (var i = 0; i < MatrixContainer.DocNameMap.Count; i++)
                {
                    var documentVector = MatrixContainer.VMatrix.Column(i).ToArray();

                    // Always do 1 minus to get the correct relation

                    var distance = Distance.Cosine(documentVector, queryVector);

                    docResults.Add(new DocResult()
                    {
                        Name = MatrixContainer.DocNameMap[i],
                        Distance = distance
                    });
                }

                foreach (var result in docResults.OrderBy(d => d.Distance).Take(5))
                {
                    Console.WriteLine(result.Name + " Distance: " + result.Distance);
                }
            }
        }

        public static Job CreateNewJob(int docCount)
        {
            using (var context = new SvdEntities())
            {
                var job = context.Jobs.Add(new Job()
                {
                    DocumentCount = docCount,
                    Created = DateTime.Now
                });

                context.SaveChanges();

                return job;
            }
        }

        public static List<Job> GetJobs()
        {
            using (var context = new SvdEntities())
            {
                context.Configuration.ProxyCreationEnabled = false;
                return context.Jobs.ToList();
            }
        }

        public static Job GetJob(int id)
        {
            using (var context = new SvdEntities())
            {
                context.Configuration.ProxyCreationEnabled = false;
                return context.Jobs.Find(id);
            }
        }

        public static List<Document> GetDocuments(int page, int docsPerPage)
        {
            using (var context = new SvdEntities())
            {
                context.Configuration.ProxyCreationEnabled = false;

                var docs = context.Documents.OrderBy(i => i.Name)/*.Skip(page * docsPerPage).Take(docsPerPage)*/.ToList();

                foreach (var doc in docs)
                {
                    doc.TotalTermDocCount = context.TermDocumentCounts.Count(tdc => tdc.DocumentId == doc.Id);
                }

                return docs;
            }
        }

        public static List<TermDocumentCount> GetTermDocumentCounts(int docId)
        {
            using (var context = new SvdEntities())
            {
                context.Configuration.ProxyCreationEnabled = false;

                return context.TermDocumentCounts.Where(tdc => tdc.DocumentId == docId).ToList();
            }
        }

        public static void GetMatrixContainer(int jobId)
        {
            if (!_matrixContainers.ContainsKey(jobId))
            {
                using (var context = new SvdEntities())
                {
                    var job = context.Jobs.Find(jobId);

                    var binaryFormatter = new BinaryFormatter();

                    DenseMatrix newUMatrix = null;
                    DenseMatrix newVMatrix = null;

                    using (var ms = new MemoryStream(job.UMatrix.SerializedValues))
                    {
                        var uValues = binaryFormatter.Deserialize(ms) as float[];

                        newUMatrix = new DenseMatrix(job.JobTerms.Count, job.Dimensions, uValues);
                    }

                    using (var ms = new MemoryStream(job.VMatrix.SerializedValues))
                    {
                        var vValues = binaryFormatter.Deserialize(ms) as float[];

                        newVMatrix = new DenseMatrix(job.Dimensions, job.JobDocuments.Count, vValues);
                    }

                    // Calc Distance Map
                    var distanceMap = new float[newVMatrix.ColumnCount, newVMatrix.ColumnCount];

                    Enumerable.Range(0, newVMatrix.ColumnCount).AsParallel().ForAll(i =>
                    {
                        for (var m = 0; m < newVMatrix.ColumnCount; m++)
                        {
                            distanceMap[i, m] = Distance.Cosine(newVMatrix.Column(i).ToArray(), newVMatrix.Column(m).ToArray());
                        }
                    });

                    _matrixContainers[jobId] = new MatrixContainer()
                    {
                        Dimensions = job.Dimensions,
                        DocNameMap = job.JobDocuments.OrderBy(jd => jd.OrdinalIndex).Select(d => d.Document.Name).ToList(),
                        Terms = job.JobTerms.OrderBy(jt => jt.OrdinalIndex).Select(t => t.Term.Value).ToList(),
                        UMatrix = newUMatrix,
                        VMatrix = newVMatrix,
                        DistanceMap = distanceMap
                    };
                }
            }

            MatrixContainer = _matrixContainers[jobId];
        }
    }
}
