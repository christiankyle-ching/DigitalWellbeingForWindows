namespace DigitalWellbeingUI.Models
{
    public class ChartDataPoint
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public ChartDataPoint(string name, int value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
