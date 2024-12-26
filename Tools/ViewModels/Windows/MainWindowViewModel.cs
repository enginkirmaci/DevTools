using Prism.Mvvm;

namespace Tools.ViewModels.Windows
{
    public class MainWindowViewModel : BindableBase
    {
        public string ApplicationTitle { get; } = "Dev Tools";

        public MainWindowViewModel()
        {
            //var sss = DateTime.Now.ToString("yyyyMMddhhmmss");
            //var files = Directory.GetFiles("C:\\Users\\201579\\Downloads\\DML", "*.*");
            //using (var output = new StreamWriter("C:\\Users\\201579\\Downloads\\DML\\_combined.txt"))
            //{
            //    foreach (var file in files)
            //    {
            //        using (var input = new StreamReader(file))
            //        {
            //            output.WriteLine($"---{Path.GetFileName(file)}");
            //            output.WriteLine("");
            //            output.WriteLine(input.ReadToEnd());
            //        }
            //    }
            //}
            //var snakeCase = "Rowid".ToSnakeCase().ToUpperInvariant();

            ////var date = DateTime.Now.ToString("O");

            //string base64Encoded = "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";

            //// Decode the Base64 string to a byte array
            //byte[] bytes = Convert.FromBase64String(base64Encoded);

            //// Define the output file path
            //string outputFilePath = @"C:\test\UK1148.p12";

            //// Write the byte array to the file
            //File.WriteAllBytes(outputFilePath, bytes);

            //var temp = Base64Decode("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");
            //var temp1 = Base64Encode("-");
            //var temp2 = Base64Encode("3Mx7_Cr4A2t");
            //var temp3 = Base64Encode("Fy13Rty5");
            //var temp4 = Base64Encode("1Hrbq66y");

            //var t2 = Base64Decode("Mv7sa5Tr");
            //var t = "3";

            //            var t = new string[] {
            //  "TrnxCode",
            //  "TrnxSubCode",
            //  "Source",
            //  "Brand",
            //  "Dci",
            //  "AtmTrnx",
            //  "MainStoreNo",
            //  "StoreNo",
            //  "BranchCode",
            //  "BatchNo",
            //  "F2",
            //  "F3Trm",
            //  "F3Sys",
            //  "F4",
            //  "F4Org",
            //  "F12",
            //  "F13",
            //  "F14",
            //  "F15",
            //  "F18",
            //  "F22",
            //  "F23",
            //  "F24",
            //  "F25",
            //  "F32",
            //  "F37",
            //  "F38",
            //  "F38Mp",
            //  "F39",
            //  "F39Auth",
            //  "F41",
            //  "F42",
            //  "F42Out",
            //  "F43Name",
            //  "F43City",
            //  "F43State",
            //  "F43Country",
            //  "F43ZipCode",
            //  "F43StreetAddress",
            //  "F44Out",
            //  "F47",
            //  "F48Out",
            //  "F48In",
            //  "F49",
            //  "F49Org",
            //  "F55T82",
            //  "F55T84",
            //  "F55T95",
            //  "F55T9a",
            //  "F55T9c",
            //  "F55T5f2a",
            //  "F55T5f34",
            //  "F55T9f02",
            //  "F55T9f03",
            //  "F55T9f09",
            //  "F55T9f10",
            //  "F55T9f1a",
            //  "F55T9f1e",
            //  "F55T9f26",
            //  "F55T9f27",
            //  "F55T9f33",
            //  "F55T9f34",
            //  "F55T9f35",
            //  "F55T9f36",
            //  "F55T9f37",
            //  "F55T9f41",
            //  "F55T9f53",
            //  "F55T9f6e",
            //  "F55T71",
            //  "F55T72",
            //  "F55T91",
            //  "F60Out",
            //  "F61",
            //  "F62",
            //  "F63McRep",
            //  "F90",
            //  "SysMti",
            //  "TrmMti",
            //  "PrivateData_124",
            //  "PrivateData_108",
            //  "CpsTxnId",
            //  "CpsValidationCode",
            //  "CpsAci",
            //  "CpsProductId",
            //  "CavvInd",
            //  "CavvResult",
            //  "Cvv2ResultCode",
            //  "IsTaxFree",
            //  "TaxFreeRate",
            //  "IsExc",
            //  "KkptRefNum",
            //  "KkptSenderName",
            //  "KkptPaymentType",
            //  "KkptSenderMessage",
            //  "KkptTranChannel",
            //  "IsAirlineTxn",
            //  "CampCode",
            //  "CampId",
            //  "CardMaximumFlag",
            //  "MrcMaximumFlag",
            //  "InstallCnt",
            //  "AdviceEntryDate",
            //  "AvsResponseCode",
            //  "RecurringFlag",
            //  "EciInd",
            //  "OrderId",
            //  "CvmType",
            //  "SubDealerCode",
            //  "SubMerchantId",
            //  "DccInd",
            //  "PosTermCap",
            //  "TxnTrm",
            //  "TermCap",
            //  "PaymentFacilitatorId",
            //  "IndependSaleOrganizationId",
            //  "AvsResultCode",
            //  "CatInd",
            //  "ProcessType",
            //  "ProcessDate",
            //  "SysEntryTime",
            //  "ProcessExecuteDate",
            //  "InsertDate",
            //  "InsertTime",
            //  "MovedFlag",
            //  "MovedDate",
            //  "ActualMerchantCountry",
            //  "Tod",
            //  "AtmNo",
            //  "IsStandartAtm",
            //  "DestAccountNumber",
            //  "OutF26",
            //  "ConversionRate",
            //  "AdditionalAmnt",
            //  "AdditionalAmntType",
            //  "F6",
            //  "F51",
            //  "F48_26",
            //  "F48_32",
            //  "F48_42",
            //  "IncF63",
            //  "F48_23",
            //  "F48_96",
            //  "F3In",
            //  "TransactionFeeAmount",
            //  "PrivateData_104",
            //  "F48_77",
            //  "IncF48_52",
            //  "MulClrSeqNo",
            //  "MulClrSeqCount",
            //  "Iban",
            //  "IsQrTransaction",
            //  "F62Bsp",
            //  "IsBsp",
            //  "CavvOtoSettlement",
            //  "F51Org",
            //  "F6Org",
            //  "Cvvrst",
            //  "Pds0001",
            //  "De16ConversionDate",
            //  "F48Se22",
            //  "AsisUlasimFlag",
            //  "AsisUlasimInfo",
            //  "F55_9f06",
            //  "F43NameForBsp",
            //  "F43CityForBsp",
            //  "F43StateForBsp",
            //  "F43CountryForBsp",
            //  "F43ZipCodeForBsp",
            //  "F43StreetAddressForBsp",
            //  "BankMaxiBonusRate",
            //  "MrcMaxiBonusRate",
            //  "CardExtraInstallCnt",
            //  "CardDelayedCnt",
            //  "MpTransactionGroup",
            //  "CardMaxiFixedTermRate",
            //  "InstallNumber",
            //  "InstallAmount",
            //  "OrgF37"
            //};
            //            var sb = new StringBuilder();
            //            foreach (var i in t)
            //            {
            //                sb.Append($"${{{i.ToSnakeCase().ToUpperInvariant()}}} {Environment.NewLine}");
            //            }

            //            var res = sb.ToString();
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}