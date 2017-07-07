using HtmlAgilityPack;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
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
        public static int NumDocs = 5000;
        //private static readonly object locker = new object();
        public static MatrixContainer MatrixContainer { get; set; }

        public static readonly HashSet<string> Exclusions = new HashSet<string>() { "me", "you", "his", "him", "her", "herself", "no", "gnu", "disclaimers", "copyrights", "navigation", "donate", "documentation", "trademark", "revision", "contact", "modified", "charity", "registered" };

        public static void ProcessAndStore()
        {
            var fileStreamDict = new StreamReader("D:/Wiki/dict.txt");

            var words = new List<string>();

            while (!fileStreamDict.EndOfStream)
            {
                words.Add(fileStreamDict.ReadLine());
            }

            words = words.Except(Exclusions).ToList();

            //var docCollection = new HashSet<Tuple<string, string>>();

            // CHECK FOR LATER (CHANGED FROM DICTIONARY - MIGHT BE OUT OF ORDER)
            //var docNameMap = new ConcurrentDictionary<int, string>();

            var readFilesStart = DateTime.Now;

            var allFiles = Directory.EnumerateFiles("D:/Wiki/", "*.html", SearchOption.AllDirectories).ToList();

            var fileCount = allFiles.Count;

            var random = new Random();

            var fileIndexes = new HashSet<int>();

            while(fileIndexes.Count <= NumDocs)
            {
                fileIndexes.Add(random.Next(fileCount));
            }

            var files = Enumerable.Range(0, NumDocs).ToList().Select(i => allFiles.ElementAt(fileIndexes.ElementAt(i))).ToList();

            var newDocCollection = new ConcurrentDictionary<string, Dictionary<string, int>>();

            files.AsParallel().ForAll((file) =>
            {
                var html = File.ReadAllText(file, Encoding.UTF8);

                HtmlDocument doc = new HtmlDocument();

                doc.LoadHtml(HttpUtility.HtmlDecode(html));

                //var tokenList = new List<string>();

                doc.DocumentNode.SelectNodes("//body//text()").ToList().ForEach(node =>
                {
                    var text = node.InnerText.Trim();

                    if (!String.IsNullOrEmpty(text) && !String.IsNullOrWhiteSpace(text))
                    {
                        var filtered = text.Where(c => (
                            char.IsLetterOrDigit(c) ||
                            char.IsWhiteSpace(c) ||
                            c == '-')).ToArray();

                        text = new string(filtered);

                        foreach (var _token in text.Trim().Split(' '))
                        {
                            var miniToken = _token.Trim().ToLower();

                            if (!string.IsNullOrEmpty(miniToken) && miniToken != "-" && miniToken != "\n" && words.Contains(miniToken))
                            {
                                var miniTokenDIct = newDocCollection.GetOrAdd(miniToken, new Dictionary<string, int>());

                                if(!miniTokenDIct.ContainsKey(file))
                                    miniTokenDIct[file] = 1;
                                else
                                    ++miniTokenDIct[file];
                            }
                        }
                    }
                });

                //var joinedTokens = string.Join(" ", tokenList);

                //lock (locker)
                //    docCollection.Add(Tuple.Create(file, joinedTokens));
            });

            var matrix = new DenseMatrix(newDocCollection.Count, NumDocs);
            var termsList = newDocCollection.Keys.ToList();

            foreach (var term in newDocCollection)
            {
                var termIndex = termsList.IndexOf(term.Key);
                foreach (var docCount in term.Value)
                {
                    matrix[termIndex, files.IndexOf(docCount.Key)] = docCount.Value;
                }
            }

            matrix.CoerceZero(.0000001);

            Debug.WriteLine($"Read File Calc Time: {DateTime.Now.Subtract(readFilesStart).TotalMilliseconds} Milliseconds");

            var svdStart = DateTime.Now;

            var svd = matrix.Svd();

            Debug.WriteLine($"SVD Calc Time: {DateTime.Now.Subtract(svdStart).TotalMilliseconds} Milliseconds");

            var dimensions = svd.S.Count <= 300 ? svd.S.Count : 300;

            // Reduction Step - U Table

            var newUMatrix = new DenseMatrix(termsList.Count, dimensions);

            for (var i = 0; i < dimensions; i++)
            {
                var singularValue = svd.S[i];

                for (var m = 0; m < termsList.Count; m++)
                {
                    newUMatrix[m, i] = svd.U[m, i] * singularValue;
                }
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

            if (!Directory.Exists("D:/Wiki/" + NumDocs))
            {
                Directory.CreateDirectory("D:/Wiki/" + NumDocs);
            }

            // V Save

            var fileStreamV = new FileStream("D:/Wiki/" + NumDocs + "/v.dat", FileMode.Create);

            var binaryFormatterV = new BinaryFormatter();

            binaryFormatterV.Serialize(fileStreamV, newVMatrix.Values);

            fileStreamV.Close();

            // U Save

            var fileStreamU = new FileStream("D:/Wiki/" + NumDocs + "/u.dat", FileMode.Create);

            var binaryFormatterU = new BinaryFormatter();

            binaryFormatterU.Serialize(fileStreamU, newUMatrix.Values);

            fileStreamU.Close();

            // Doc Map Save

            var fileStreamDocMap = new FileStream("D:/Wiki/" + NumDocs + "/docMap.dat", FileMode.Create);

            var binaryFormatterDocMap = new BinaryFormatter();

            binaryFormatterDocMap.Serialize(fileStreamDocMap, files);

            fileStreamDocMap.Close();

            // Term-Index Map Save

            var fileStreamTermsMap = new FileStream("D:/Wiki/" + NumDocs + "/termsMap.dat", FileMode.Create);

            var binaryFormatterTermsMap = new BinaryFormatter();

            binaryFormatterTermsMap.Serialize(fileStreamTermsMap, termsList);

            fileStreamTermsMap.Close();

            // Save Dimensions

            var dimensionStream = new FileStream("D:/Wiki/" + NumDocs + "/dimensions.dat", FileMode.Create);

            var dimensionFormatted = new BinaryFormatter();

            dimensionFormatted.Serialize(dimensionStream, dimensions);

            dimensionStream.Close();
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

            MatrixContainer = new MatrixContainer()
            {
                Dimensions = dimensions,
                DocNameMap = docNameMap,
                Terms = termsList,
                UMatrix = newUMatrix,
                VMatrix = newVMatrix
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
        public Dictionary<Tuple<int, int>, float> DistanceMap { get; set; }
    }
}
