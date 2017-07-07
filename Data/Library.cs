using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class Library
    {
        public class LSA
        {
            public static int NumDocs = 3000;

            //static void Main(string[] args)
            //{
            //    //ProcessAndStore();
            //    //RunQueryFromFile("hydrogen");
            //    Cluster();
            //}

            public static void ProcessAndStore()
            {
                var fileStreamDict = new StreamReader("D:/Wiki/dict.txt");

                var words = new List<string>();

                while (!fileStreamDict.EndOfStream)
                {
                    words.Add(fileStreamDict.ReadLine());
                }

                var docCollection = new List<string>();

                var docNameMap = new Dictionary<int, string>();

                var counter = 0;

                foreach (var file in Directory.EnumerateFiles("D:/Wiki/", "*.html", SearchOption.AllDirectories))
                {
                    var html = File.ReadAllText(file, Encoding.UTF8);

                    HtmlDocument doc = new HtmlDocument();

                    doc.LoadHtml(HttpUtility.HtmlDecode(html));

                    var tokenList = new List<string>();

                    foreach (var node in doc.DocumentNode.SelectNodes("//body//text()"))
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
                                var miniToken = _token.Trim();

                                if (!string.IsNullOrEmpty(miniToken) && miniToken != "-" && miniToken != "\n" && words.Contains(miniToken.ToLower()))
                                //if (!string.IsNullOrEmpty(miniToken) && miniToken != "-" && miniToken != "\n")
                                {
                                    tokenList.Add(miniToken);
                                }
                            }
                        }
                    }

                    docCollection.Add(string.Join(" ", tokenList));

                    docNameMap[counter] = file;

                    if (++counter == NumDocs) break;
                }

                // Create term/doc data structure

                var termDict = new Dictionary<string, Dictionary<int, int>>();

                for (var i = 0; i < docCollection.Count; i++)
                {
                    var tokens = docCollection[i].Split(' ');

                    for (var m = 0; m < tokens.Length; m++)
                    {
                        var token = tokens[m];

                        var docIdentifier = (i + 1);

                        Dictionary<int, int> docTermCountsRecord;

                        if (termDict.TryGetValue(token, out docTermCountsRecord) == false)
                        {
                            termDict.Add(token, new Dictionary<int, int>() { { docIdentifier, 1 } });
                        }
                        else
                        {
                            int countsForDoc;

                            if (docTermCountsRecord.TryGetValue(docIdentifier, out countsForDoc) == false)
                            {
                                docTermCountsRecord.Add(docIdentifier, 1);
                            }
                            else
                            {
                                docTermCountsRecord[docIdentifier] = ++countsForDoc;
                            }

                            termDict[token] = docTermCountsRecord;
                        }
                    }
                }

                // Build Dense Matrix

                var matrix = new DenseMatrix(termDict.Count, docCollection.Count);

                for (var i = 0; i < docCollection.Count; i++)
                {
                    for (var m = 0; m < termDict.Keys.Count; m++)
                    {
                        var term = termDict.Keys.ElementAt(m);

                        var termCounts = termDict[term];

                        int countsForDoc;

                        if (termCounts.TryGetValue(i + 1, out countsForDoc) == false)
                        {
                            matrix[m, i] = 0;
                        }
                        else
                        {
                            matrix[m, i] = countsForDoc;
                        }
                    }
                }

                var svd = matrix.Svd();

                var dimensions = svd.S.Count <= 300 ? svd.S.Count : 300;

                // Reduction Step - U Table

                var newUMatrix = new DenseMatrix(termDict.Count, dimensions);

                for (var i = 1; i <= dimensions; i++)
                {
                    var singularValue = svd.S[i - 1];

                    for (var m = 0; m < termDict.Count; m++)
                    {
                        newUMatrix[m, i - 1] = svd.U[m, i - 1] * singularValue;
                    }
                }

                // Reduction Step - V Table

                var newVMatrix = new DenseMatrix(dimensions, docCollection.Count);

                for (var i = 1; i <= dimensions; i++)
                {
                    var singularValue = svd.S[i - 1];

                    for (var m = 0; m < docCollection.Count; m++)
                    {
                        newVMatrix[i - 1, m] = svd.VT[i - 1, m] * singularValue;
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

                binaryFormatterDocMap.Serialize(fileStreamDocMap, docNameMap);

                fileStreamDocMap.Close();

                // Term-Index Map Save

                var fileStreamTermsMap = new FileStream("D:/Wiki/" + NumDocs + "/termsMap.dat", FileMode.Create);

                var binaryFormatterTermsMap = new BinaryFormatter();

                var keysArray = termDict.Keys.ToArray();

                binaryFormatterTermsMap.Serialize(fileStreamTermsMap, keysArray);

                fileStreamDocMap.Close();

                // Save Dimensions

                var dimensionStream = new FileStream("D:/Wiki/" + NumDocs + "/dimensions.dat", FileMode.Create);

                var dimensionFormatted = new BinaryFormatter();

                dimensionFormatted.Serialize(dimensionStream, dimensions);

                dimensionStream.Close();
            }

            public static Dictionary<int, float[]> Distances { get; set; }
            public static Dictionary<int, int> ClusterMap { get; set; }
            public static Dictionary<int, float[]> Centers { get; set; }
            public static int Clusters = 10;

            public static MatrixContainer MatrixContainer { get; set; }

            public static void CalcDistances()
            {
                for (var i = 0; i < Clusters; i++)
                {
                    for (var m = 0; m < MatrixContainer.VMatrix.ColumnCount; m++)
                    {
                        var targetDocVector = MatrixContainer.VMatrix.Column(m).ToArray();

                        if (m == 0)
                        {
                            Distances[i] = new float[MatrixContainer.VMatrix.ColumnCount];
                        }

                        Distances[i][m] = Distance.Cosine(Centers[i], targetDocVector);
                    }
                }
            }

            public static void Assign()
            {
                for (var i = 0; i < MatrixContainer.VMatrix.ColumnCount; i++)
                {
                    var clusterAssignment = 0;

                    var closestDistance = Distances[0][i];

                    for (var m = 1; m < Clusters; m++)
                    {
                        if (Distances[m][i] < closestDistance)
                        {
                            closestDistance = Distances[m][i];
                            clusterAssignment = m;
                        }
                    }

                    ClusterMap[i] = clusterAssignment;
                }
            }

            public static void MoveCenters()
            {
                var isOptimizedMap = new bool[Clusters];

                for (var i = 0; i < Clusters; i++)
                {
                    var clusterTotal = ClusterMap.Count(c => c.Value == i);

                    if (clusterTotal > 0)
                    {
                        DenseVector vectorSum = null;

                        for (var m = 0; m < MatrixContainer.VMatrix.ColumnCount; m++)
                        {
                            if (ClusterMap[m] == i)
                            {
                                if (vectorSum == null)
                                {
                                    vectorSum = (DenseVector)MatrixContainer.VMatrix.Column(m);
                                }
                                else
                                {
                                    vectorSum += (DenseVector)MatrixContainer.VMatrix.Column(m);
                                }
                            }
                        }

                        var newCenter = (vectorSum / clusterTotal).ToArray();

                        if (Centers[i] != null)
                        {
                            isOptimizedMap[i] = Distance.Cosine(Centers[i], newCenter) < OptimizationVarianceThreshold;

                            Console.WriteLine("Cluster #" + i + ": " + Distance.Cosine(Centers[i], newCenter));
                        }

                        Centers[i] = newCenter;
                    }
                }

                IsOptimized = isOptimizedMap.All(v => v == true);
            }

            public static bool IsOptimized { get; set; }
            public static float OptimizationVarianceThreshold { get; set; }
            public static int MaxIterations { get; set; }

            public static void Cluster()
            {
                Centers = new Dictionary<int, float[]>();
                ClusterMap = new Dictionary<int, int>();
                Distances = new Dictionary<int, float[]>();
                IsOptimized = false;
                OptimizationVarianceThreshold = .00000003F;
                MaxIterations = 2000;

                GetMatrixContainer();

                var randGen = new Random();

                for (var i = 0; i < Clusters; i++)
                {
                    Centers[i] = MatrixContainer.VMatrix.Column(randGen.Next(1, NumDocs)).ToArray();
                }

                var counter = 0;

                while (!IsOptimized && counter < MaxIterations)
                {
                    CalcDistances();

                    Assign();

                    MoveCenters();

                    counter++;
                }

                Console.WriteLine("Iterations: " + counter);

                Console.ReadLine();
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

                var docNameMap = (Dictionary<int, string>)binaryFormatterDocMap.Deserialize(fileStreamDocMap);

                fileStreamDocMap.Close();

                // Term-Index Map Get

                var fileStreamTermsMap = new FileStream("D:/Wiki/" + NumDocs + "/termsMap.dat", FileMode.Open);

                var binaryFormatterTermsMap = new BinaryFormatter();

                var termsList = (string[])binaryFormatterTermsMap.Deserialize(fileStreamTermsMap);

                fileStreamDocMap.Close();

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
            public Dictionary<int, string> DocNameMap { get; set; }
            public string[] Terms { get; set; }
            public DenseMatrix UMatrix { get; set; }
            public DenseMatrix VMatrix { get; set; }
        }
    }
}
