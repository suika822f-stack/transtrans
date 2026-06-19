namespace TransTrans.Models;

public class Cauldron
{
    public List<Card> Cards { get; set; } = [];

    public EnvironmentType Environment
    {
        get
        {
            var elements = Cards.OfType<ElementCard>().ToList();
            var fire = elements.Count(x => x.Element == ElementType.Fire);
            var water = elements.Count(x => x.Element == ElementType.Water);
            var air = elements.Count(x => x.Element == ElementType.Air);
            var earth = elements.Count(x => x.Element == ElementType.Earth);
            var max = Math.Max(Math.Max(fire, water), Math.Max(air, earth));

            if (max == 0)
            {
                return EnvironmentType.None;
            }

            var maxElementCount = 0;
            if (fire == max) maxElementCount++;
            if (water == max) maxElementCount++;
            if (air == max) maxElementCount++;
            if (earth == max) maxElementCount++;

            if (maxElementCount > 1)
            {
                return EnvironmentType.None;
            }

            if (fire == max) return EnvironmentType.Fire;
            if (water == max) return EnvironmentType.Water;
            if (air == max) return EnvironmentType.Air;
            return EnvironmentType.Earth;
        }
    }
}
