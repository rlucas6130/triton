﻿using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;

namespace Engine
{
    public class Contracts
    {
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
}