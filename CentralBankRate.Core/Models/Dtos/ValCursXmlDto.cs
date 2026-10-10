using System.Xml.Serialization;

namespace CentralBankRate.Core.Models.Dtos;

[XmlRoot("ValCurs")]
public class ValCursXmlDto
{
    [XmlAttribute("Date")]
    public string? DateText { get;  set; }
    [XmlElement("Valute")]
    public List<ValuteXmlDto> Rates { get; set; } = new();
    [XmlAttribute("name")]
    public string Name { get;  set; }
    // На некорректный запрос ЦБ отвечает HTTP 200 и телом <ValCurs>Error in parameters</ValCurs>
    [XmlText]
    public string? ErrorText { get; set; }
}