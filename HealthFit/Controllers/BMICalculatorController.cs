using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Controllers
{
    public class BMICalculatorController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult BMICalculatorResult(string ddlHeight, string txtHeight, string txtWeight, string rdbGender)
        {
            var result = new
            {
                txtYourBmi = "",
                lblMessage = "",
                lblIdealWeight1 = "",
                lblIdealWeight2 = ""
            };

            try
            {
                if (!string.IsNullOrEmpty(txtHeight) && !string.IsNullOrEmpty(txtWeight) && !string.IsNullOrEmpty(ddlHeight))
                {
                    double height = Convert.ToDouble(txtHeight.Replace("'", "").Replace(",", "."));
                    if (ddlHeight == "Inches")
                    {
                        height = height * 2.54;
                    }
                    else if (ddlHeight == "Feet")
                    {
                        string[] splitFeet = txtHeight.Split('.');
                        double feet = Convert.ToDouble(splitFeet[0]);
                        double inches = splitFeet.Length > 1 ? Convert.ToDouble(splitFeet[1]) : 0;
                        height = (feet * 12 + inches) * 2.54;
                    }

                    double weight = Convert.ToDouble(txtWeight.Replace("'", "").Replace(",", "."));

                    if (weight > 0 && height > 0)
                    {
                        double bmi = weight / Math.Pow(height / 100, 2);
                        string gender = rdbGender?.ToUpper() ?? "";

                        string lblMessage = "";
                        double idealMin = 0, idealMax = 0;

                        if (gender == "FEMALE")
                        {
                            if (bmi < 19)
                                lblMessage = "Bạn đang thiếu cân.";
                            else if (bmi <= 24)
                                lblMessage = "Bạn có cân nặng bình thường.";
                            else
                                lblMessage = "Bạn đang thừa cân.";

                            idealMin = 19 * Math.Pow(height / 100, 2);
                            idealMax = 24 * Math.Pow(height / 100, 2);
                        }
                        else // MALE
                        {
                            if (bmi < 20)
                                lblMessage = "Bạn đang thiếu cân.";
                            else if (bmi <= 25)
                                lblMessage = "Bạn có cân nặng bình thường.";
                            else
                                lblMessage = "Bạn đang thừa cân.";

                            idealMin = 20 * Math.Pow(height / 100, 2);
                            idealMax = 25 * Math.Pow(height / 100, 2);
                        }

                        result = new
                        {
                            txtYourBmi = Math.Round(bmi, 2).ToString(),
                            lblMessage = lblMessage,
                            lblIdealWeight1 = Math.Round(idealMin, 2) + " KG",
                            lblIdealWeight2 = Math.Round(idealMax, 2) + " KG"
                        };
                    }
                    else
                    {
                        result = new
                        {
                            txtYourBmi = "",
                            lblMessage = "Vui lòng nhập giá trị hợp lệ",
                            lblIdealWeight1 = "",
                            lblIdealWeight2 = ""
                        };
                    }
                }
                else
                {
                    result = new
                    {
                        txtYourBmi = "",
                        lblMessage = "Vui lòng nhập giá trị hợp lệ",
                        lblIdealWeight1 = "",
                        lblIdealWeight2 = ""
                    };
                }
            }
            catch
            {
                result = new
                {
                    txtYourBmi = "",
                    lblMessage = "Có lỗi khi tính toán BMI",
                    lblIdealWeight1 = "",
                    lblIdealWeight2 = ""
                };
            }

            return Json(result);
        }
    }
} 