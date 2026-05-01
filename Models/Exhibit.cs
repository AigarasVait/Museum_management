namespace Museum_management.Models
{
    public class Exhibit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Length { get; set; }   // in centimeters (cm)
        public decimal Width { get; set; }    // in centimeters (cm)
        public decimal Height { get; set; }   // in centimeters (cm)
    }
}