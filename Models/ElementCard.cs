namespace TransTrans.Models;

public class ElementCard : Card
{
    public override bool CanPutIntoCauldron => true;

    public ElementType Element { get; set; }
}
