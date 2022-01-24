using Aspose.Pdf.Text;
using Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MachineLearning.Business
{

    public class AsposePdfTextReader
    {
        AsposeExtractImageOperation _imageOperation;
        public AsposePdfTextReader(AsposeExtractImageOperation imageOperation)
        {
            _imageOperation = imageOperation;
        }
        public List<ResponseEntity> FindeksUploadPDFFile()
        {
            string filePath = @"C:\Users\Mt801268\Desktop\FindeksRapor_42350562_4019407D11J_1632296578325.pdf";

            List<ResponseEntity> result = new List<ResponseEntity>();


            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Aspose.Pdf.Document pdfDocument = new Aspose.Pdf.Document(stream);
                MemoryStream docStream = new MemoryStream();
                pdfDocument.Save(docStream);
                byte[] bytes = docStream.ToArray();
                string byteTostring = Convert.ToBase64String(bytes, 0, bytes.Length);
                TextAbsorber textAbsorber = new TextAbsorber();
                pdfDocument.Pages.Accept(textAbsorber);
                string extractedText = textAbsorber.Text;
                string[] stringSeparators = new string[] { "RİSK RAPORU" };
                string[] splitedText = extractedText.Split(stringSeparators, StringSplitOptions.None);
                DateTime reportDate = Convert.ToDateTime(splitedText[1].Substring(splitedText[1].IndexOf("RAPOR TARİHİ"), 36).Trim().Replace("RAPOR TARİHİ", string.Empty).Trim());

                List<Bank> bankResult = _imageOperation.GetFindeksDocData(filePath);



                for (int i = 0; i < splitedText.Length; i++)
                {
                    if (splitedText[i].Contains("Limit / Risk Tablosu"))
                    {
                        ResponseEntity findeksRiskReport = new ResponseEntity();

                        bool canAdd1 = false;
                        bool canAdd2 = false;
                        bool canAdd3 = false;

                        string[] subSplit = new string[] { "\r\n\r\n" };
                        string[] subSplitedText = splitedText[i].Split(subSplit, StringSplitOptions.None);

                        for (int j = 0; j < subSplitedText.Length; j++)
                        {
                            if (subSplitedText[j].Contains("Nakdi") && !subSplitedText[j].Contains("Gayri Nakdi"))
                            {
                                string[] nakdiSplit = new string[] { "Nakdi" };
                                var nakdiSplitedText = subSplitedText[j].Split(nakdiSplit, StringSplitOptions.None);

                                if (nakdiSplitedText.Length > 0)
                                {
                                    if (nakdiSplitedText[nakdiSplitedText.Length - 1].Trim().Contains("-"))
                                    {
                                        findeksRiskReport.Cash = 0;
                                        canAdd1 = true;
                                    }
                                    else
                                    {
                                        findeksRiskReport.Cash = Convert.ToDecimal(nakdiSplitedText[nakdiSplitedText.Length - 1].Replace("Nakdi", string.Empty).Trim());
                                        canAdd1 = true;
                                    }
                                }
                            }

                            else if (subSplitedText[j].Contains("Gayri Nakdi"))
                            {
                                string[] gayrinakdiSplit = new string[] { "Gayri Nakdi" };
                                var gayrinakdiSplitedText = subSplitedText[j].Split(gayrinakdiSplit, StringSplitOptions.None);

                                if (gayrinakdiSplitedText.Length > 0)
                                {
                                    if (gayrinakdiSplitedText[gayrinakdiSplitedText.Length - 1].Trim().Contains("-"))
                                    {
                                        findeksRiskReport.NonCash = 0;
                                        canAdd2 = true;
                                    }
                                    else
                                    {
                                        findeksRiskReport.NonCash = Convert.ToDecimal(gayrinakdiSplitedText[gayrinakdiSplitedText.Length - 1].Replace("Nakdi", string.Empty).Trim());
                                        canAdd2 = true;
                                    }
                                }

                            }

                            else if (subSplitedText[j].Contains("Toplam") && !subSplitedText[j].Contains("Genel Revize Vadesi"))
                            {
                                findeksRiskReport.GeneralLimit = 0;
                                if (subSplitedText[j].Trim().Replace("Toplam", String.Empty).Trim().Replace("Diğer", String.Empty).StartsWith("-"))
                                {
                                    canAdd3 = true;
                                    findeksRiskReport.GeneralLimit = 0;
                                }
                                else
                                {
                                    canAdd3 = true;
                                    findeksRiskReport.GeneralLimit = Convert.ToDecimal(subSplitedText[j].Trim().Replace("Toplam", String.Empty).Trim().Replace("Diğer", String.Empty).Substring(0, subSplitedText[j].Trim().Replace("Toplam", String.Empty).Trim().Replace("Diğer", String.Empty).IndexOf(" ")));
                                }
                            }

                            if (canAdd1 && canAdd2 && canAdd3)
                            {
                                findeksRiskReport.FindeksDate = reportDate;
                                findeksRiskReport.TotalRisk = findeksRiskReport.Cash + findeksRiskReport.NonCash;
                                result.Add(findeksRiskReport);
                                findeksRiskReport = new ResponseEntity();
                                canAdd1 = false;
                                canAdd2 = false;
                                canAdd3 = false;
                            }
                        }
                    }
                }

                decimal total = result.Sum(r => r.TotalRisk);

                for (int i = 0; i < bankResult.Count(); i++)
                {
                    result[i].RelationBanks = bankResult[i].BankNames;
                    result[i].LimitCurrency = "TRY";
                    result[i].Share = Convert.ToDouble(Math.Round(result[i].TotalRisk * 100 / total));
                }
            }
            return result;
        }
    }

}
