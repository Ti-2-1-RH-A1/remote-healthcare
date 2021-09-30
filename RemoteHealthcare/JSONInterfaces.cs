namespace VirtualReality
{
    public interface IClientinfo
    {
        public string host { get; set; }
        public string user { get; set; }
        public string file { get; set; }
        public string renderer { get; set; }
    }

    public interface IFP
    {
        public double time { get; set; }
        public double fps { get; set; }
    }
}