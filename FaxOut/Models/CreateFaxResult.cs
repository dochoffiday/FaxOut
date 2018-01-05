using System.Collections.Generic;

namespace FaxOut
{
    public class CreateFaxResult
    {
        public List<KeyValuePair<string, string>> Errors { get; set; } = new List<KeyValuePair<string, string>>();
        public int Id { get; set; }
    }
}