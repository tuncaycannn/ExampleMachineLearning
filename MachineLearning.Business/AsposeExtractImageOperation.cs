using Aspose.Pdf;
using Entities;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace MachineLearning.Business
{

    public class AsposeExtractImageOperation
    {
        List<Bank> bankList;
        string folder;
        string path;
        DirectoryInfo filepath;
        Document pdfDocument;
        List<XImage> xImage;
        public AsposeExtractImageOperation(List<Bank> banks, Document document, List<XImage> xImages)
        {
            bankList = banks;
            folder = Directory.GetCurrentDirectory() + @"/BankLogo";
            path = Guid.NewGuid().ToString();
            filepath = Directory.CreateDirectory($"{folder}/{path}");
            pdfDocument = document;
            xImage = xImages;
        }
        public List<Bank> GetFindeksDocData(string fileContent)
        {
            int pageNumber = pdfDocument.Pages.Count;

            for (int i = 1; i <= pageNumber; i++)
            {
                int image = pdfDocument.Pages[i].Resources.Images.Count();
                if (image != 0)
                {
                    bool specPageControl = false;
                    if (i == 6 && !specPageControl)
                    {
                        if (pdfDocument.Pages[i].Resources.Images.Count() == 6)
                        {
                            specPageControl = true;
                            xImage.Add(pdfDocument.Pages[i].Resources.Images[5]);
                            xImage.Add(pdfDocument.Pages[i].Resources.Images[1]);
                            xImage.Add(pdfDocument.Pages[i].Resources.Images[3]);
                        }
                        else if (pdfDocument.Pages[i].Resources.Images.Count() == 4)
                        {
                            specPageControl = true;
                            xImage.Add(pdfDocument.Pages[i].Resources.Images[3]);
                            xImage.Add(pdfDocument.Pages[i].Resources.Images[1]);
                        }
                        else if (pdfDocument.Pages[i].Resources.Images.Count() == 2)
                        {
                            specPageControl = true;
                            xImage.Add(pdfDocument.Pages[i].Resources.Images[1]);
                        }
                    }
                    else
                    {
                        for (int j = 1; j <= image; j++)
                        {
                            if (pdfDocument.Pages[i].Resources.Images[j].ContainsTransparency == true)
                            {
                                xImage.Add(pdfDocument.Pages[i].Resources.Images[j]);
                            }
                        }
                    }
                    specPageControl = false;
                }
            }

            for (int i = 0; i < xImage.Count; i++)
            {
                FileStream outputImage = new FileStream(filepath.FullName + @"/" + i + ".png", FileMode.Create);
                xImage[i].Save(outputImage, ImageFormat.Png);
                outputImage.Close();
            }

            DirectoryInfo info = new DirectoryInfo(filepath.FullName);
            var list = new List<ImagePath>();

            foreach (FileInfo item in info.GetFiles().OrderByNatural(x => x.FullName))
            {
                list.Add(new ImagePath
                {
                    Source = File.ReadAllBytes(item.FullName)
                });
            }

            for (int i = 0; i < list.Count; i++)
            {
                MLModel1.ModelInput sampleData = new MLModel1.ModelInput()
                {
                    ImageSource = list[i].Source,
                };
                var predictionResult = MLModel1.Predict(sampleData);

                bankList.Add(new Bank
                {
                    BankNames = predictionResult.PredictedLabel
                });
            }

            info.Delete(true);

            return bankList;
        }
    }
}
