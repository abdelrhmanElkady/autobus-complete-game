namespace autobus_complete.Models
{
    public class Group
    {
        public Group(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public List<User> Users { get; set; } = new List<User>();


    }
}
