namespace WorkService.Models
{
    public struct Job
    {
        public Job(string name, string parameters, bool running)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.Running = running;
        }

        public string Name { get; private set; }

        public string Parameters { get; private set; }

        public bool Running { get; private set; }
    }
}
