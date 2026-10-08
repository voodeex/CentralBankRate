using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Services.Parser;

public interface IParser
{
    
   Result<ValCurs> Parse(string data) ;
}