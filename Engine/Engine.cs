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

namespace Engine
{
    public static class LSA
    {
        public static int NumDocs = 1000;
        public static int JobId = 12;
        public static string DictionaryPath = "D:/Wiki/dict.txt";

        public static MatrixContainer MatrixContainer { get; set; }

        public static readonly HashSet<string> Exclusions = new HashSet<string>() { "me", "you", "his", "him", "her", "herself", "no", "gnu", "disclaimers", "copyrights", "navigation", "donate", "documentation", "trademark", "revision", "contact", "modified", "charity", "registered" };

        public static IEnumerable<Term> GetOrAddTerms(SvdEntities context)
        {
            var terms = context.Terms.Where(t => !Exclusions.Contains(t.Value)).ToList();

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

        public static DenseMatrix GetTermDocMatrix(SvdEntities context, Job job)
        {
            var termLookup = GetOrAddTerms(context).ToLookup(t => t.Value);

            SetJobStatus(context, job, JobStatus.BuildingMatrix);

            var readFilesStart = DateTime.Now;

            var allFiles = Directory.EnumerateFiles("D:/Wiki/", "*.html", SearchOption.AllDirectories).ToList();

            var fileCount = allFiles.Count;

            var random = new Random();

            var fileIndexes = new HashSet<int>();

            while (fileIndexes.Count <= NumDocs)
            {
                fileIndexes.Add(random.Next(fileCount));
            }

            var files = Enumerable.Range(0, NumDocs).ToList().Select(i => allFiles.ElementAt(fileIndexes.ElementAt(i))).ToList();

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
                    // THIS MIGHT CAUSE AN ISSUE????
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
                         group tdc by new { DocumentId = tdc.Document.Id, TermId = tdc.Term.Id } into g
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

            var matrix = new DenseMatrix(termsList.Count, NumDocs);

            foreach (var termDocCount in termDocCounts)
            {
                matrix[termsList.IndexOf(termDocCount.Term.Value), files.IndexOf(termDocCount.Document.Name)] = termDocCount.Count;
            }

            matrix.CoerceZero(.0000001);

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
                    AddTermDocumentCount(termList.First(), document, termDocCounts);
                }
            }
        }

        public static void AddTermDocumentCount(Term term, Document document, ConcurrentBag<TermDocumentCount> termDocCounts)
        {
            termDocCounts.Add(new TermDocumentCount()
            {
                Document = document,
                Term = term
            });
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

        public static void ProcessAndStore()
        {
            Job job = null;

            using (var context = new SvdEntities())
            {
                try
                {
                    // Create Job With All default values (must set dimensions if different than default '300')
                    job = context.Jobs.Add(new Job()
                    {
                        DocumentCount = NumDocs,
                        Created = DateTime.Now
                    });

                    context.SaveChanges();

                    // Process
                    var matrix = GetTermDocMatrix(context, job);
                    var svd = GetSvd(context, job, matrix);

                    var dimensions = svd.S.Count <= 300 ? svd.S.Count : 300;
                     
                    var binaryFormatter = new BinaryFormatter();

                    // Reduction Step - U Table

                    var newUMatrix = new DenseMatrix(matrix.RowCount, dimensions);

                    for (var i = 0; i < dimensions; i++)
                    {
                        var singularValue = svd.S[i];

                        for (var m = 0; m < matrix.RowCount; m++)
                        {
                            newUMatrix[m, i] = svd.U[m, i] * singularValue;
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

                    var newVMatrix = new DenseMatrix(dimensions, NumDocs);

                    for (var i = 0; i < dimensions; i++)
                    {
                        for (var m = 0; m < NumDocs; m++)
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

                    //var jobDocsLookup = job.JobDocuments.ToLookup(d => d.OrdinalIndex);
                    //var jobDocDistances = new List<JobDocumentDistance>();

                    //for(var i = 0; i < newVMatrix.ColumnCount; i++)
                    //{
                    //    var sourceDoc = jobDocsLookup[i].First();

                    //    for (var m = 0; m < newVMatrix.ColumnCount; m++)
                    //    {
                    //        if (m == i) continue;

                    //        var targetDoc = jobDocsLookup[m].First();

                    //        jobDocDistances.Add(new JobDocumentDistance()
                    //        {
                    //            JobId = job.Id,
                    //            SourceJobDocumentId = sourceDoc.Id,
                    //            TargetJobDocumentId = targetDoc.Id,
                    //            Distance = Distance.Cosine(newVMatrix.Column(i).ToArray(), newVMatrix.Column(m).ToArray())
                    //        });
                    //    }
                    //}

                    //using (var bulkInsertContext = new SvdEntities())
                    //{
                    //    bulkInsertContext.JobDocumentDistances.AddRange(jobDocDistances);
                    //    bulkInsertContext.BulkSaveChanges(false);
                    //}

                    job.Dimensions = dimensions;
                    job.Status = JobStatus.Complete;

                    context.SaveChanges();
                }
                catch (Exception)
                {
                    job.Status = JobStatus.Failed;
                    context.SaveChanges();

                    throw;
                }
            }
        }

        public static void RunQueryFromFile(string query)
        {
            GetMatrixContainer();

            //// Locate Query index in U Table

            var queryIndex = MatrixContainer.Terms.ToList().IndexOf(query);

            var queryVector = MatrixContainer.UMatrix.Row(queryIndex).ToArray();

            Console.WriteLine("Query: " + query);
            Console.WriteLine("Query Vector: " + queryVector.ToString());
            Console.WriteLine("Dimensions: " + MatrixContainer.Dimensions);

            var docResults = new List<DocResult>();

            for (var i = 0; i < MatrixContainer.DocNameMap.Count; i++)
            {
                var documentVector = MatrixContainer.VMatrix.Column(i).ToArray();

                // Always do 1 minus to get the correct relation

                var distance = 1 - Distance.Cosine(documentVector, queryVector);

                docResults.Add(new DocResult()
                {
                    Name = MatrixContainer.DocNameMap[i],
                    Distance = distance
                });
            }

            foreach (var result in docResults.OrderByDescending(d => Math.Abs(d.Distance)).Take(5))
            {
                Console.WriteLine(result.Name + " Distance: " + result.Distance);
            }

            Console.ReadLine();
        }

        public static void GetMatrixContainer()
        {
            using (var context = new SvdEntities())
            {
                var job = context.Jobs.Find(JobId);

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

                MatrixContainer = new MatrixContainer()
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
    }
}
