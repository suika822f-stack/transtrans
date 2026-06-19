namespace TransTrans.Models;

public class Card
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public bool IsReady { get; set; }

    public virtual bool CanPutIntoCauldron => false;
    public virtual bool CanUse => false;
}
