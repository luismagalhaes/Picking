using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

[ServiceContract]
public interface IServiceRest
{
    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, 
                UriTemplate = "GetState")]
    IsAlive GetState();
    
    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, 
                UriTemplate = "GetDateTime")]
    DataHora GetDateTime();

    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, 
                UriTemplate = "GetListPicks/{dataIni=null}/{dataFim=null}/{operador=null}/{posto=null}")]
    List<PicagemDetalhes> GetListPicks(string dataIni, string dataFim, string operador, string posto);

    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, 
                UriTemplate = "GetListPosts")]
    List<PostoPicagem> GetListPosts();

    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
                UriTemplate = "GetListStatePick")]
    List<PicagemEstado> GetListStatePick();

    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, 
                UriTemplate = "GetOperator/{numero}")]
    Operador GetOperator(string numero);

    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
               UriTemplate = "GetJustificationPick/{operador}/{dataNova}")]
    PicagemJustificacao GetJustificationPick(string operador, string dataNova);

    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, 
                UriTemplate = "GetListAction")]
    List<AccaoSobrePicagem> GetListAction();

    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
                UriTemplate = "GetListTimming")]
    List<TurnoTempos> GetListTimming();

    [OperationContract]
    [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
              UriTemplate = "PostSynchronizationPicks")]
    bool PostSynchronizationPicks(Pick[] pick);

    [OperationContract]
    [WebInvoke(Method = "POST",
                UriTemplate = "PostInsertRetifyPick",RequestFormat = WebMessageFormat.Json,ResponseFormat = WebMessageFormat.Json)]
    bool PostInsertRetifyPick(PicagemJustificacao pic);
}
