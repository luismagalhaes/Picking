using System.ServiceModel;

[ServiceContract]
public interface IServiceSoap
{
    [OperationContract]
    bool IsAliveSoap();
}
