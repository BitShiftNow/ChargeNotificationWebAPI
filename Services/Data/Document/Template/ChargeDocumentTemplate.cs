using System.Text;
using System.Xml.Serialization;
using WebAPI.Models;

namespace WebAPI.Services.Data.Document.Schema;

public class ChargeDocumentTemplate {
    public Header? Header;
    public Body? Body;
}

public class Header {
    [XmlArray]
    [XmlArrayItem(typeof(HorizontalLine))]
    [XmlArrayItem(typeof(Text))]
    public Item[] Items = [];
}

public class Body {
    [XmlArray]
    [XmlArrayItem(typeof(HorizontalLine))]
    [XmlArrayItem(typeof(Text))]
    [XmlArrayItem(typeof(ChargeTable))]
    public Item[] Items = [];
}

public abstract class Item {
}

public class HorizontalLine : Item {
    [XmlAttribute]
    public float Thickness = 1.0f;
}

public class TextColor {
    [XmlAttribute]
    public byte A = 0xFF;

    [XmlAttribute]
    public byte R = 0;

    [XmlAttribute]
    public byte G = 0;

    [XmlAttribute]
    public byte B = 0;
}

public class Text : Item {
    [XmlAttribute]
    public string Style = "Normal"; // Normal, Bold, Italic

    [XmlAttribute]
    public string Alignment = "Left"; // Left, Right, Center

    [XmlAttribute]
    public float Size = 12.0f;

    public TextColor Color = new();
    public string Value = string.Empty;
}

public class ChargeTable : Item {
    public Text[] Heading = [];
    public Text[] Cells = [];
}
