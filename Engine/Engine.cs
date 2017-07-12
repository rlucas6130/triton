using HtmlAgilityPack;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Engine
{
    public static class LSA
    {
        public static int NumDocs = 20;
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
            var terms = GetOrAddTerms(context);
            var termLookup = terms.ToLookup(t => t.Value);

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

            var documentEntities = new List<Document>();
            var newDocuments = new List<Document>();
            var jobDocuments = new List<JobDocument>();

            // Create Documents
            foreach(var file in files)
            {
                var docEntity = context.Documents.FirstOrDefault(d => d.Name == file);

                if(docEntity == null)
                {
                    docEntity = new Document()
                    {
                        Name = file
                    };

                    newDocuments.Add(docEntity);
                }

                jobDocuments.Add(new JobDocument()
                {
                    Job = job,
                    Document = docEntity, 
                    // THIS MIGHT CAUSE AN ISSUE????
                    OrdinalIndex = files.IndexOf(file)
                });

                documentEntities.Add(docEntity);
            }

            context.Documents.AddRange(newDocuments);
            context.JobDocuments.AddRange(jobDocuments);

            context.SaveChanges();

            // Setup Parallel Collections
            ConcurrentBag<TermDocumentCount> termDocCountsBag = new ConcurrentBag<TermDocumentCount>();
            ConcurrentBag<TermDocumentCount> termDocCountsBagCalculated = new ConcurrentBag<TermDocumentCount>();

            documentEntities.AsParallel().ForAll((documentEntity) =>
            {
                if (documentEntity.TermDocumentCounts.Count > 0)
                {
                    documentEntity.TermDocumentCounts.ToList().ForEach(tdc => termDocCountsBag.Add(tdc));
                }
                else
                {
                    var html = File.ReadAllText(documentEntity.Name, Encoding.UTF8);

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

                            ParseDocumentData(text, documentEntity, termDocCountsBagCalculated, termLookup);
                        }
                    });
                }
            });

            var termDocCounts = termDocCountsBag.ToList();

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

            termDocCounts.AddRange(newTdc);

            var termsList = termDocCounts.Select(tdc => tdc.Term.Value).Distinct().ToList();
            var matrix = new DenseMatrix(termsList.Count, NumDocs);
            
            // Save Job Terms
            var jobTerms = from t in termsList
                           let termEntity = termLookup[t].First()
                           select new JobTerm()
                           {
                               Job = job,
                               TermId = termEntity.Id,
                               OrdinalIndex = termsList.IndexOf(t)
                           };

            context.JobTerms.AddRange(jobTerms);

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

            //var termDocCount = termDocCounts.FirstOrDefault(tdc => tdc.Document.Name == document.Name && tdc.Term.Value == term.Value);

            //if(termDocCount == null)
            //    termDocCounts.Add(new TermDocumentCount()
            //    {
            //        Document = document,
            //        Term = term,
            //        Count = 1
            //    });
            // else
            //    ++termDocCount.Count;

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

                    job.Dimensions = dimensions;

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
            // Get Dimensions

            var dimensionStream = new FileStream("D:/Wiki/" + NumDocs + "/dimensions.dat", FileMode.Open);

            var dimensionFormatter = new BinaryFormatter();

            var dimensions = (int)dimensionFormatter.Deserialize(dimensionStream);

            dimensionStream.Close();

            // Doc Map Get

            var fileStreamDocMap = new FileStream("D:/Wiki/" + NumDocs + "/docMap.dat", FileMode.Open);

            var binaryFormatterDocMap = new BinaryFormatter();

            var docNameMap = (List<string>)binaryFormatterDocMap.Deserialize(fileStreamDocMap);

            fileStreamDocMap.Close();

            // Term-Index Map Get

            var fileStreamTermsMap = new FileStream("D:/Wiki/" + NumDocs + "/termsMap.dat", FileMode.Open);

            var binaryFormatterTermsMap = new BinaryFormatter();

            var termsList = (List<string>)binaryFormatterTermsMap.Deserialize(fileStreamTermsMap);

            fileStreamTermsMap.Close();

            // V Get

            var fileStreamV = new FileStream("D:/Wiki/" + NumDocs + "/v.dat", FileMode.Open);

            var binaryFormatterV = new BinaryFormatter();

            float[] vValues = (float[])binaryFormatterV.Deserialize(fileStreamV);

            fileStreamV.Close();

            var newVMatrix = new DenseMatrix(dimensions, docNameMap.Count, vValues);

            // U Get

            var fileStreamU = new FileStream("D:/Wiki/" + NumDocs + "/u.dat", FileMode.Open);

            var binaryFormatterU = new BinaryFormatter();

            float[] uValues = (float[])binaryFormatterU.Deserialize(fileStreamU);

            fileStreamU.Close();

            var newUMatrix = new DenseMatrix(termsList.Count(), dimensions, uValues);

            // Calc Distance Map
            var distanceMap = new float[newVMatrix.ColumnCount,newVMatrix.ColumnCount];

            Enumerable.Range(0, newVMatrix.ColumnCount).AsParallel().ForAll(i =>
            {
                for (var m = 0; m < newVMatrix.ColumnCount; m++)
                {
                    distanceMap[i, m] = Distance.Cosine(newVMatrix.Column(i).ToArray(), newVMatrix.Column(m).ToArray());
                }
            });

            MatrixContainer = new MatrixContainer()
            {
                Dimensions = dimensions,
                DocNameMap = docNameMap,
                Terms = termsList,
                UMatrix = newUMatrix,
                VMatrix = newVMatrix,
                DistanceMap = distanceMap
            };
        }
    }

    public class DocResult
    {
        public string Name { get; set; }
        public double Distance { get; set; }
    }

    public class MatrixContainer
    {
        public int Dimensions { get; set; }
        public List<string> DocNameMap { get; set; }
        public List<string> Terms { get; set; }
        public DenseMatrix UMatrix { get; set; }
        public DenseMatrix VMatrix { get; set; }
        public float[,] DistanceMap { get; set; }
    }
}
