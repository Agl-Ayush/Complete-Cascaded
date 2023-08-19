namespace join.Models
{
    public class product
    {
        public int id { get; set; }
        public string? name { get; set; }
        public int? phone { get; set; }
        public string? address { get; set; }

        public string? country { get; set; }

        public string? state { get; set; }

        public string? city { get; set; }

        public int? cityId { get; set; }

        public int? CountryId { get; set; }

        public int? StateId { get; set; }

        public string? file_name { get; set; }
        public string? file_content_type { get; set; }

        public IFormFile? ProductFile { get; set; }

        public byte[]? FileContent { get; set; }

    }

    public class Country
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class State
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? CountryId { get; set; }
    }

    public class City
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? StateId { get; set; }
    }

}
