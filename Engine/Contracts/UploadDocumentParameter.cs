using System;

namespace Engine.Contracts
{
    [Serializable]
    public class UploadDocumentParameter
    {
        public string FileName { get; set; }
        public byte[] StreamData { get; set; }
    }
}
