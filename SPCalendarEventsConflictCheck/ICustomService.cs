using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ISAPI.Interfaces
{
    [ServiceContract]
    interface ICustomService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle =
            WebMessageBodyStyle.WrappedRequest, UriTemplate = "SejneSobe_PreveriPrekrivanje")]
        ServiceResult<bool> SejneSobe_PreveriPrekrivanje(string SiteUrl, string ListGuid, string DatumOd, string DatumDo, string ID = null);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle =
            WebMessageBodyStyle.WrappedRequest, UriTemplate = "SejneSobe_PreveriPrekrivanjePonavljajoce")]
        ServiceResult<bool> SejneSobe_PreveriPrekrivanjePonavljajoce(string siteUrl, string listGuid, string uraOd, string uraDo, int tipPonovitve, string dnevnoTipVzorec, int dnevnoVsakihDni, int tedenskoVsakihTednov, string dnevi, string mesecnoTipVzorec, int mesecnoDan, int mesecnoVsakihMesecev, string mesecnoKateri, string mesecnoDan2, int mesecnoVsakihMesecev2, string letnoTipVzorec, int letnoVsakMesec, int letnoDan, string letnoKateri, string letnoDan2, int letnoVMesecu, string datumOd, string tipDatumDo, string datumDo, int stPonovitev, string ID = null);
    }
}
