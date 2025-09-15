namespace TimeCafe.UI.Utilities;

public class CrossTemplateItem
{
    public double Xpx, Ypx;
    public double Size;
    public double Angle;
}

public static class CrossTemplates
{
    public const double BaseWidth = 500;
    public const double BaseHeight = 375;
    private const double MinDistance = 60;
    private const int ItemsPerTemplate = 15;
    private const int TemplateCount = 25;

    public static readonly List<List<CrossTemplateItem>> Templates;

    static CrossTemplates()
    {
        var rnd = new Random(0);
        Templates = new List<List<CrossTemplateItem>>(TemplateCount);

        for (int t = 0; t < TemplateCount; t++)
        {
            var list = new List<CrossTemplateItem>();
            int attempts = 0;

            while (list.Count < ItemsPerTemplate && attempts < 1000)
            {
                attempts++;

                double x = rnd.NextDouble() * BaseWidth;
                double y = rnd.NextDouble() * BaseHeight;

                bool tooClose = list.Any(p =>
                {
                    double dx = p.Xpx - x;
                    double dy = p.Ypx - y;
                    return Math.Sqrt(dx * dx + dy * dy) < MinDistance;
                });
                if (tooClose) continue;

                list.Add(new CrossTemplateItem
                {
                    Xpx = x,
                    Ypx = y,
                    Size = rnd.Next(26, 63),
                    Angle = rnd.NextDouble() * 360
                });
            }

            Templates.Add(list);
        }
    }
}
