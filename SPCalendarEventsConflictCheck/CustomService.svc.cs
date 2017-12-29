using Microsoft.SharePoint;
using Microsoft.SharePoint.Client.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Activation;
using System.Xml;
using System.Globalization;
using System.Linq;

namespace ISAPI.Concrete
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [BasicHttpBindingServiceMetadataExchangeEndpoint]
    public class CustomService : ICustomService
    {
        public ServiceResult<bool> SejneSobe_PreveriPrekrivanje(string SiteUrl, string ListGuid, string DatumOd, string DatumDo, string ID = null)
        {
            var serviceResult = ServiceResult<bool>.Default;
            serviceResult = ServiceResult<bool>.Get(false);

            try
            {
                DateTime datumOd = DateTime.ParseExact(DatumOd, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime datumDo = DateTime.ParseExact(DatumDo, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                using (SPSite oSPSite = new SPSite(SiteUrl))
                {
                    using (SPWeb oSPWeb = oSPSite.OpenWeb())
                    {
                        SPList list = oSPWeb.Lists[new Guid(ListGuid)];

                        SPQuery query = new SPQuery();
                        query.ExpandRecurrence = true;

                        DateTime zacMesec = new DateTime(datumOd.Year, datumOd.Month, 1, 0, 0, 0);

                        DateTime konMesec = new DateTime(datumDo.Year, datumDo.Month, 1, 0, 0, 0);
                        konMesec = konMesec.AddMonths(1).AddSeconds(-1);

                        bool ponavljajoci = false;
                        if (!String.IsNullOrEmpty(ID) && ID.Contains("."))
                            ponavljajoci = true;

                        while (zacMesec < konMesec)
                        {
                            query.CalendarDate = zacMesec;

                            if (String.IsNullOrEmpty(ID))
                                query.Query = "<Where><DateRangesOverlap><FieldRef Name=\"EventDate\" /><FieldRef Name=\"EndDate\" /><Value Type=\"DateTime\"><Month /></Value></DateRangesOverlap></Where>";
                            else
                                query.Query = "<Where><And><Neq><FieldRef Name=\'ID\'/><Value Type=\'Number\'>" + ID + "</Value></Neq><DateRangesOverlap><FieldRef Name=\"EventDate\" /><FieldRef Name=\"EndDate\" /><Value Type=\"DateTime\"><Month /></Value></DateRangesOverlap></And></Where>";

                            SPListItemCollection items;
                            items = list.GetItems(query);

                            foreach (SPListItem item in items)
                            {
                                if (ponavljajoci && item.RecurrenceID.Equals(ID))
                                    continue;

                                DateTime startDate = DateTime.Parse(item["EventDate"].ToString());
                                DateTime endDate = DateTime.Parse(item["EndDate"].ToString());

                                if (
                                    (
                                        (
                                            (startDate > datumOd) && (startDate < datumDo)
                                        ) &&
                                        (endDate >= datumDo)
                                    ) ||
                                    (
                                        (
                                            (startDate <= datumOd) && ((endDate > datumOd) && (endDate < datumDo))
                                        ) ||
                                        (
                                            ((startDate <= datumOd) && (endDate >= datumDo)) ||
                                            ((startDate >= datumOd) && (endDate <= datumDo))
                                        )
                                    )
                                   )
                                {
                                    serviceResult = ServiceResult<bool>.Get(true);
                                    return serviceResult;
                                }
                            }

                            zacMesec = zacMesec.AddMonths(1);
                        }
                    }
                }
            }
            catch (Exception _ex)
            {
                serviceResult = ServiceResult<bool>.Error(_ex.ToString());
            }

            return serviceResult;
        }

        public ServiceResult<bool> SejneSobe_PreveriPrekrivanjePonavljajoce(string siteUrl, string listGuid, string uraOd, string uraDo, int tipPonovitve, string dnevnoTipVzorec, int dnevnoVsakihDni, int tedenskoVsakihTednov, string dnevi, string mesecnoTipVzorec, int mesecnoDan, int mesecnoVsakihMesecev, string mesecnoKateri, string mesecnoDan2, int mesecnoVsakihMesecev2, string letnoTipVzorec, int letnoVsakMesec, int letnoDan, string letnoKateri, string letnoDan2, int letnoVMesecu, string datumOd, string tipDatumDo, string datumDo, int stPonovitev, string ID = null)
        {
            var serviceResult = ServiceResult<bool>.Default;
            serviceResult = ServiceResult<bool>.Get(false);

            try
            {
                int maxStLet = 1;
                int maxStLetMesec = 1;
                int maxStLetLet = 5;

                TimeSpan UraOd = TimeSpan.ParseExact(uraOd, "hh\\:mm", CultureInfo.InvariantCulture);
                TimeSpan UraDo = TimeSpan.ParseExact(uraDo, "hh\\:mm", CultureInfo.InvariantCulture);

                // DatumOd
                DateTime DatumOd = DateTime.ParseExact(datumOd, "d.M.yyyy", CultureInfo.InvariantCulture);
                DatumOd = DatumOd.Add(UraOd);

                List<DogodekItem> dogodki = new List<DogodekItem>();

                DateTime tempDatumOd;

                switch (tipPonovitve)
                {
                    case 2: // Dnevno

                        #region DNEVNO
                        switch (dnevnoTipVzorec)
                        {
                            case "dailyRecurType0": // vsakih X dni

                                switch (tipDatumDo)
                                {
                                    case "endDateRangeType0": // brez končnega datuma
                                        DateTime DatumDo = DatumOd.AddYears(maxStLet);

                                        while (DatumOd < DatumDo)
                                        {
                                            dogodki.Add(new DogodekItem()
                                            {
                                                Od = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                Do = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                            });

                                            DatumOd = DatumOd.AddDays(dnevnoVsakihDni);
                                        }

                                        break;
                                    case "endDateRangeType1": // število ponovitev
                                        int tStPonovitev = 0;
                                        while (tStPonovitev < stPonovitev)
                                        {
                                            dogodki.Add(new DogodekItem()
                                            {
                                                Od = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                Do = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                            });

                                            tStPonovitev++;
                                            DatumOd = DatumOd.AddDays(dnevnoVsakihDni);
                                        }

                                        break;
                                    case "endDateRangeType2": // končni datum
                                        DatumDo = DateTime.ParseExact(datumDo, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                                        while (DatumOd < DatumDo)
                                        {
                                            dogodki.Add(new DogodekItem()
                                            {
                                                Od = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                Do = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                            });

                                            DatumOd = DatumOd.AddDays(dnevnoVsakihDni);
                                        }

                                        break;
                                }

                                break;
                            case "dailyRecurType1": // Vsak delovnik

                                while (DatumOd.DayOfWeek == DayOfWeek.Saturday || DatumOd.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    DatumOd = DatumOd.AddDays(dnevnoVsakihDni);
                                }

                                switch (tipDatumDo)
                                {
                                    case "endDateRangeType0": // brez končnega datuma
                                        DateTime DatumDo = DatumOd.AddYears(maxStLet);

                                        while (DatumOd < DatumDo)
                                        {
                                            dogodki.Add(new DogodekItem()
                                            {
                                                Od = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                Do = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                            });

                                            do
                                            {
                                                DatumOd = DatumOd.AddDays(dnevnoVsakihDni);
                                            }
                                            while (DatumOd.DayOfWeek == DayOfWeek.Saturday || DatumOd.DayOfWeek == DayOfWeek.Sunday);
                                        }

                                        break;
                                    case "endDateRangeType1": // število ponovitev

                                        int tStPonovitev = 0;
                                        while (tStPonovitev < stPonovitev)
                                        {
                                            dogodki.Add(new DogodekItem()
                                            {
                                                Od = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                Do = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                            });

                                            tStPonovitev++;
                                            do
                                            {
                                                DatumOd = DatumOd.AddDays(dnevnoVsakihDni);
                                            }
                                            while (DatumOd.DayOfWeek == DayOfWeek.Saturday || DatumOd.DayOfWeek == DayOfWeek.Sunday);
                                        }

                                        break;
                                    case "endDateRangeType2": // končni datum

                                        DatumDo = DateTime.ParseExact(datumDo, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                                        while (DatumOd < DatumDo)
                                        {
                                            dogodki.Add(new DogodekItem()
                                            {
                                                Od = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                Do = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                            });

                                            do
                                            {
                                                DatumOd = DatumOd.AddDays(dnevnoVsakihDni);
                                            }
                                            while (DatumOd.DayOfWeek == DayOfWeek.Saturday || DatumOd.DayOfWeek == DayOfWeek.Sunday);
                                        }

                                        break;
                                }

                                break;
                        }
                        #endregion

                        break;
                    case 3: // Tedensko

                        #region TEDENSKO

                        bool[] dneviSplit = dnevi.Split(',').Select(x => x == "1").ToArray();

                        tempDatumOd = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second);

                        switch (tipDatumDo)
                        {
                            case "endDateRangeType0": // brez končnega datuma

                                DateTime DatumDo = DatumOd.AddYears(maxStLet);

                                var zDatumOd = DatumOd;
                                while (DatumOd < DatumDo)
                                {
                                    for (int i = 0; i < dneviSplit.Length; i++)
                                    {
                                        if (dneviSplit[i])
                                        {
                                            DateTime izbDatum = DatumOd.AddDays(-(int)DatumOd.DayOfWeek + i);
                                            if ((izbDatum >= zDatumOd) && (izbDatum <= DatumDo))
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }
                                        }
                                    }

                                    DatumOd = DatumOd.AddDays(7 * tedenskoVsakihTednov);
                                }

                                break;
                            case "endDateRangeType1": // število ponovitev

                                int tStPonovitev = 0;

                                zDatumOd = DatumOd;
                                while (tStPonovitev < stPonovitev)
                                {
                                    for (int i = 0; i < dneviSplit.Length; i++)
                                    {
                                        if (dneviSplit[i])
                                        {
                                            DateTime izbDatum = DatumOd.AddDays(-(int)DatumOd.DayOfWeek + i);
                                            if (izbDatum >= zDatumOd)
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }
                                        }
                                    }

                                    tStPonovitev++;
                                    DatumOd = DatumOd.AddDays(7 * tedenskoVsakihTednov);
                                }

                                break;
                            case "endDateRangeType2": // končni datum
                                DatumDo = DateTime.ParseExact(datumDo, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                                zDatumOd = DatumOd;
                                while (DatumOd < DatumDo)
                                {
                                    for (int i = 0; i < dneviSplit.Length; i++)
                                    {
                                        if (dneviSplit[i])
                                        {
                                            DateTime izbDatum = DatumOd.AddDays(-(int)DatumOd.DayOfWeek + i);
                                            if ((izbDatum >= zDatumOd) && (izbDatum <= DatumDo))
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }
                                        }
                                    }

                                    DatumOd = DatumOd.AddDays(7 * tedenskoVsakihTednov);
                                }

                                break;
                        }

                        #endregion

                        break;
                    case 4: // Mesečno

                        #region MESEČNO

                        tempDatumOd = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second);

                        switch (mesecnoTipVzorec)
                        {
                            case "monthlyRecurType0": // kateri dan

                                switch (tipDatumDo)
                                {
                                    case "endDateRangeType0": // brez končnega datuma

                                        DateTime DatumDo = DatumOd.AddYears(maxStLetMesec);

                                        var zDatumOd = DatumOd;
                                        while (DatumOd < DatumDo)
                                        {
                                            DateTime izbDatum = new DateTime(DatumOd.Year, DatumOd.Month, mesecnoDan);
                                            if ((izbDatum >= zDatumOd) && (izbDatum <= DatumDo))
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            DatumOd = DatumOd.AddMonths(mesecnoVsakihMesecev);
                                        }

                                        break;
                                    case "endDateRangeType1": // število ponovitev

                                        int tStPonovitev = 0;

                                        zDatumOd = DatumOd;
                                        while (tStPonovitev < stPonovitev)
                                        {
                                            DateTime izbDatum = new DateTime(DatumOd.Year, DatumOd.Month, mesecnoDan);
                                            if (izbDatum >= zDatumOd)
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            tStPonovitev++;
                                            DatumOd = DatumOd.AddMonths(mesecnoVsakihMesecev);
                                        }

                                        break;
                                    case "endDateRangeType2": // končni datum
                                        DatumDo = DateTime.ParseExact(datumDo, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                                        zDatumOd = DatumOd;
                                        while (DatumOd < DatumDo)
                                        {
                                            DateTime izbDatum = new DateTime(DatumOd.Year, DatumOd.Month, mesecnoDan);
                                            if ((izbDatum >= zDatumOd) && (izbDatum <= DatumDo))
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            DatumOd = DatumOd.AddMonths(mesecnoVsakihMesecev);
                                        }

                                        break;
                                }

                                break;
                            case "monthlyRecurType1": // katera ponovitev (prvi itd)

                                int kateriDan = -1;
                                switch (mesecnoKateri)
                                {
                                    case "first":
                                        kateriDan = 1;
                                        break;
                                    case "second":
                                        kateriDan = 2;
                                        break;
                                    case "third":
                                        kateriDan = 3;
                                        break;
                                    case "fourth":
                                        kateriDan = 4;
                                        break;
                                    case "last":
                                        kateriDan = 5;
                                        break;
                                }

                                switch (tipDatumDo)
                                {
                                    case "endDateRangeType0": // brez končnega datuma

                                        DateTime DatumDo = DatumOd.AddYears(maxStLetMesec);

                                        var zDatumOd = DatumOd;
                                        while (DatumOd < DatumDo)
                                        {
                                            var izbDatum = IzracunajDatumMeseca(mesecnoDan2, kateriDan, DatumOd).Value;
                                            if ((izbDatum >= zDatumOd) && (izbDatum <= DatumDo))
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            DatumOd = DatumOd.AddMonths(mesecnoVsakihMesecev2);
                                        }

                                        break;
                                    case "endDateRangeType1": // število ponovitev

                                        int tStPonovitev = 0;

                                        zDatumOd = DatumOd;
                                        while (tStPonovitev < stPonovitev)
                                        {
                                            var izbDatum = IzracunajDatumMeseca(mesecnoDan2, kateriDan, DatumOd).Value;
                                            if (izbDatum >= zDatumOd)
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            tStPonovitev++;
                                            DatumOd = DatumOd.AddMonths(mesecnoVsakihMesecev2);
                                        }

                                        break;
                                    case "endDateRangeType2": // končni datum
                                        DatumDo = DateTime.ParseExact(datumDo, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                                        zDatumOd = DatumOd;
                                        while (DatumOd < DatumDo)
                                        {
                                            var izbDatum = IzracunajDatumMeseca(mesecnoDan2, kateriDan, DatumOd).Value;
                                            if ((izbDatum >= zDatumOd) && (izbDatum <= DatumDo))
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            DatumOd = DatumOd.AddMonths(mesecnoVsakihMesecev2);
                                        }

                                        break;
                                }

                                break;
                        }

                        #endregion

                        break;
                    case 5: // Letno

                        #region LETNO

                        tempDatumOd = new DateTime(DatumOd.Year, DatumOd.Month, DatumOd.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second);

                        switch (letnoTipVzorec)
                        {
                            case "yearlyRecurType0": // kateri dan

                                switch (tipDatumDo)
                                {
                                    case "endDateRangeType0": // brez končnega datuma

                                        DateTime DatumDo = DatumOd.AddYears(maxStLetLet);

                                        var zDatumOd = DatumOd;
                                        while (DatumOd < DatumDo)
                                        {
                                            DateTime izbDatum = new DateTime(DatumOd.Year, letnoVsakMesec, letnoDan);
                                            if ((izbDatum >= zDatumOd) && (izbDatum <= DatumDo))
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            DatumOd = DatumOd.AddYears(1);
                                        }

                                        break;
                                    case "endDateRangeType1": // število ponovitev

                                        int tStPonovitev = 0;

                                        zDatumOd = DatumOd;
                                        while (tStPonovitev < stPonovitev)
                                        {
                                            DateTime izbDatum = new DateTime(DatumOd.Year, letnoVsakMesec, letnoDan);
                                            if (izbDatum >= zDatumOd)
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            tStPonovitev++;
                                            DatumOd = DatumOd.AddYears(1);
                                        }

                                        break;
                                    case "endDateRangeType2": // končni datum
                                        DatumDo = DateTime.ParseExact(datumDo, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                                        zDatumOd = DatumOd;
                                        while (DatumOd < DatumDo)
                                        {
                                            DateTime izbDatum = new DateTime(DatumOd.Year, letnoVsakMesec, letnoDan);
                                            if ((izbDatum >= zDatumOd) && (izbDatum <= DatumDo))
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            DatumOd = DatumOd.AddYears(1);
                                        }

                                        break;
                                }

                                break;
                            case "yearlyRecurType1": // katera ponovitev (prvi itd)

                                int kateriDan = -1;
                                switch (letnoKateri)
                                {
                                    case "first":
                                        kateriDan = 1;
                                        break;
                                    case "second":
                                        kateriDan = 2;
                                        break;
                                    case "third":
                                        kateriDan = 3;
                                        break;
                                    case "fourth":
                                        kateriDan = 4;
                                        break;
                                    case "last":
                                        kateriDan = 5;
                                        break;
                                }

                                switch (tipDatumDo)
                                {
                                    case "endDateRangeType0": // brez končnega datuma

                                        DateTime DatumDo = DatumOd.AddYears(maxStLetLet);

                                        var zDatumOd = DatumOd;
                                        while (DatumOd < DatumDo)
                                        {
                                            var izbDatum = IzracunajDatumLeta(letnoDan2, kateriDan, letnoVMesecu, DatumOd).Value;
                                            if ((izbDatum >= zDatumOd) && (izbDatum <= DatumDo))
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            DatumOd = DatumOd.AddYears(1);
                                        }

                                        break;
                                    case "endDateRangeType1": // število ponovitev

                                        int tStPonovitev = 0;

                                        zDatumOd = DatumOd;
                                        while (tStPonovitev < stPonovitev)
                                        {
                                            var izbDatum = IzracunajDatumLeta(letnoDan2, kateriDan, letnoVMesecu, DatumOd).Value;
                                            if (izbDatum >= zDatumOd)
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            tStPonovitev++;
                                            DatumOd = DatumOd.AddYears(1);
                                        }

                                        break;
                                    case "endDateRangeType2": // končni datum
                                        DatumDo = DateTime.ParseExact(datumDo, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                                        zDatumOd = DatumOd;
                                        while (DatumOd < DatumDo)
                                        {
                                            var izbDatum = IzracunajDatumLeta(letnoDan2, kateriDan, letnoVMesecu, DatumOd).Value;
                                            if ((izbDatum >= zDatumOd) && (izbDatum <= DatumDo))
                                            {
                                                dogodki.Add(new DogodekItem()
                                                {
                                                    Od = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, DatumOd.Hour, DatumOd.Minute, DatumOd.Second),
                                                    Do = new DateTime(izbDatum.Year, izbDatum.Month, izbDatum.Day, UraDo.Hours, UraDo.Minutes, UraDo.Seconds)
                                                });
                                            }

                                            DatumOd = DatumOd.AddYears(1);
                                        }

                                        break;
                                }

                                break;
                        }

                        #endregion

                        break;
                }

                if (dogodki.Count > 0)
                {
                    using (SPSite oSPSite = new SPSite(siteUrl))
                    {
                        using (SPWeb oSPWeb = oSPSite.OpenWeb())
                        {
                            SPList list = oSPWeb.Lists[new Guid(listGuid)];

                            SPQuery query = new SPQuery();
                            query.ExpandRecurrence = true;

                            var minDogodek = dogodki.OrderBy(x => x.Od).First();
                            DateTime zacMesec = new DateTime(minDogodek.Od.Year, minDogodek.Od.Month, 1, 0, 0, 0);

                            var maxDogodek = dogodki.OrderByDescending(x => x.Do).First();
                            DateTime konMesec = new DateTime(maxDogodek.Do.Year, maxDogodek.Do.Month, 1, 0, 0, 0);
                            konMesec = konMesec.AddMonths(1).AddSeconds(-1);

                            while (zacMesec < konMesec)
                            {
                                query.CalendarDate = zacMesec;

                                if (String.IsNullOrEmpty(ID))
                                    query.Query = "<Where><DateRangesOverlap><FieldRef Name=\"EventDate\" /><FieldRef Name=\"EndDate\" /><Value Type=\"DateTime\"><Month /></Value></DateRangesOverlap></Where>";
                                else
                                    query.Query = "<Where><And><Neq><FieldRef Name=\'ID\'/><Value Type=\'Number\'>" + ID + "</Value></Neq><DateRangesOverlap><FieldRef Name=\"EventDate\" /><FieldRef Name=\"EndDate\" /><Value Type=\"DateTime\"><Month /></Value></DateRangesOverlap></And></Where>";

                                SPListItemCollection items;
                                items = list.GetItems(query);

                                foreach (SPListItem item in items)
                                {
                                    DateTime startDate = DateTime.Parse(item["EventDate"].ToString());
                                    DateTime endDate = DateTime.Parse(item["EndDate"].ToString());

                                    if (dogodki.Where(x => (
                                        (
                                            (
                                                (startDate > x.Od) && (startDate < x.Do)
                                            ) &&
                                            (endDate >= x.Do)
                                        ) ||
                                        (
                                            (
                                                (startDate <= x.Od) && ((endDate > x.Od) && (endDate < x.Do))
                                            ) ||
                                            (
                                                ((startDate <= x.Od) && (endDate >= x.Do)) ||
                                                ((startDate >= x.Od) && (endDate <= x.Do))
                                            )
                                        )
                                       )).Count() > 0)
                                    {
                                        serviceResult = ServiceResult<bool>.Get(true);
                                        return serviceResult;
                                    }
                                }

                                zacMesec = zacMesec.AddMonths(1);
                            }
                        }
                    }
                }
            }
            catch (Exception _ex)
            {
                serviceResult = ServiceResult<bool>.Error(_ex.ToString());
            }

            return serviceResult;
        }

        private DateTime? IzracunajDatumMeseca(string mesecnoDan2, int kateriDan, DateTime DatumOd)
        {
            switch (mesecnoDan2)
            {
                case "day":
                    if (kateriDan < 5)
                        return new DateTime(DatumOd.Year, DatumOd.Month, kateriDan);
                    else
                    {
                        var tempDate = new DateTime(DatumOd.Year, DatumOd.Month + 1, 1);
                        return tempDate.AddDays(-1);
                    }
                case "weekday":
                    var izbDan = 1;
                    if (kateriDan < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Monday, kateriDan);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Monday, MondaysInMonth(DatumOd, DayOfWeek.Monday));
                    return new DateTime(DatumOd.Year, DatumOd.Month, izbDan);
                case "weekend_day":
                    izbDan = 1;
                    switch (kateriDan)
                    {
                        case 1:
                            izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Saturday, 1);
                            break;
                        case 2:
                            izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Sunday, 1);
                            break;
                        case 3:
                            izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Saturday, 2);
                            break;
                        case 4:
                            izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Sunday, 2);
                            break;
                        case 5:
                            var izbDanSa = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Saturday, MondaysInMonth(DatumOd, DayOfWeek.Saturday));
                            var izbDanSu = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Sunday, MondaysInMonth(DatumOd, DayOfWeek.Sunday));
                            izbDan = Math.Max(izbDanSa, izbDanSu);
                            break;
                    }
                    return new DateTime(DatumOd.Year, DatumOd.Month, izbDan);
                case "su":
                    if (kateriDan < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Sunday, kateriDan);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Sunday, MondaysInMonth(DatumOd, DayOfWeek.Sunday));
                    return new DateTime(DatumOd.Year, DatumOd.Month, izbDan);
                case "mo":
                    if (kateriDan < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Monday, kateriDan);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Monday, MondaysInMonth(DatumOd, DayOfWeek.Monday));
                    return new DateTime(DatumOd.Year, DatumOd.Month, izbDan);
                case "tu":
                    if (kateriDan < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Tuesday, kateriDan);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Tuesday, MondaysInMonth(DatumOd, DayOfWeek.Tuesday));
                    return new DateTime(DatumOd.Year, DatumOd.Month, izbDan);
                case "we":
                    if (kateriDan < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Wednesday, kateriDan);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Wednesday, MondaysInMonth(DatumOd, DayOfWeek.Wednesday));
                    return new DateTime(DatumOd.Year, DatumOd.Month, izbDan);
                case "th":
                    if (kateriDan < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Thursday, kateriDan);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Thursday, MondaysInMonth(DatumOd, DayOfWeek.Thursday));
                    return new DateTime(DatumOd.Year, DatumOd.Month, izbDan);
                case "fr":
                    if (kateriDan < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Friday, kateriDan);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Friday, MondaysInMonth(DatumOd, DayOfWeek.Friday));
                    return new DateTime(DatumOd.Year, DatumOd.Month, izbDan);
                case "sa":
                    if (kateriDan < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Saturday, kateriDan);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, DatumOd.Month, DayOfWeek.Saturday, MondaysInMonth(DatumOd, DayOfWeek.Saturday));
                    return new DateTime(DatumOd.Year, DatumOd.Month, izbDan);
            }

            return null;
        }

        private DateTime? IzracunajDatumLeta(string letnoDan2, int letnoKateri, int mesec, DateTime DatumOd)
        {
            switch (letnoDan2)
            {
                case "day":
                    if (letnoKateri < 5)
                        return new DateTime(DatumOd.Year, mesec, letnoKateri);
                    else
                    {
                        var tempDate = new DateTime(DatumOd.Year + 1, mesec, 1);
                        return tempDate.AddDays(-1);
                    }
                case "weekday":
                    var izbDan = 1;
                    if (letnoKateri < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Monday, letnoKateri);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Monday, MondaysInMonth(DatumOd, DayOfWeek.Monday));
                    return new DateTime(DatumOd.Year, mesec, izbDan);
                case "weekend_day":
                    izbDan = 1;
                    switch (letnoKateri)
                    {
                        case 1:
                            izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Saturday, 1);
                            break;
                        case 2:
                            izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Sunday, 1);
                            break;
                        case 3:
                            izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Saturday, 2);
                            break;
                        case 4:
                            izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Sunday, 2);
                            break;
                        case 5:
                            var izbDanSa = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Saturday, MondaysInMonth(DatumOd, DayOfWeek.Saturday));
                            var izbDanSu = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Sunday, MondaysInMonth(DatumOd, DayOfWeek.Sunday));
                            izbDan = Math.Max(izbDanSa, izbDanSu);
                            break;
                    }
                    return new DateTime(DatumOd.Year, mesec, izbDan);
                case "su":
                    if (letnoKateri < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Sunday, letnoKateri);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Sunday, MondaysInMonth(DatumOd, DayOfWeek.Sunday));
                    return new DateTime(DatumOd.Year, mesec, izbDan);
                case "mo":
                    if (letnoKateri < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Monday, letnoKateri);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Monday, MondaysInMonth(DatumOd, DayOfWeek.Monday));
                    return new DateTime(DatumOd.Year, mesec, izbDan);
                case "tu":
                    if (letnoKateri < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Tuesday, letnoKateri);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Tuesday, MondaysInMonth(DatumOd, DayOfWeek.Tuesday));
                    return new DateTime(DatumOd.Year, mesec, izbDan);
                case "we":
                    if (letnoKateri < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Wednesday, letnoKateri);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Wednesday, MondaysInMonth(DatumOd, DayOfWeek.Wednesday));
                    return new DateTime(DatumOd.Year, mesec, izbDan);
                case "th":
                    if (letnoKateri < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Thursday, letnoKateri);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Thursday, MondaysInMonth(DatumOd, DayOfWeek.Thursday));
                    return new DateTime(DatumOd.Year, mesec, izbDan);
                case "fr":
                    if (letnoKateri < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Friday, letnoKateri);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Friday, MondaysInMonth(DatumOd, DayOfWeek.Friday));
                    return new DateTime(DatumOd.Year, mesec, izbDan);
                case "sa":
                    if (letnoKateri < 5)
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Saturday, letnoKateri);
                    else
                        izbDan = DayFinder.FindDay(DatumOd.Year, mesec, DayOfWeek.Saturday, MondaysInMonth(DatumOd, DayOfWeek.Saturday));
                    return new DateTime(DatumOd.Year, mesec, izbDan);
            }

            return null;
        }

        private int MondaysInMonth(DateTime thisMonth, DayOfWeek dan)
        {
            int mondays = 0;
            int month = thisMonth.Month;
            int year = thisMonth.Year;
            int daysThisMonth = DateTime.DaysInMonth(year, month);
            DateTime beginingOfThisMonth = new DateTime(year, month, 1);
            for (int i = 0; i < daysThisMonth; i++)
                if (beginingOfThisMonth.AddDays(i).DayOfWeek == dan)
                    mondays++;
            return mondays;
        }
    }
}