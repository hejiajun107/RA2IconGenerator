using ImageMagick;
using Newtonsoft.Json;
using SkiaSharp;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection.PortableExecutable;


namespace IconGenerator
{
    public partial class form1 : Form
    {
        private IconConfig IconConfig;

        string outputDir = "./output";

        public form1()
        {
            using(var sr =new StreamReader("./IconConfig.json"))
            {
                var str = sr.ReadToEnd();

                IconConfig = JsonConvert.DeserializeObject<IconConfig>(str);
            }

            InitializeComponent();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            if(!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            foreach(var file in files)
            {
                using(var bitmap = DrawIcon(file, false))
                {
                    bitmap.SavePcxOrPng(outputDir + "/" + Path.GetFileNameWithoutExtension(file) + $"icon.{IconConfig.OutPutType.ToString().ToLower()}");
                }
                using (var bitmap = DrawIcon(file, true))
                {
                    bitmap.SavePcxOrPng(outputDir + "/" + Path.GetFileNameWithoutExtension(file) + $"uicon.{IconConfig.OutPutType.ToString().ToLower()}");
                }
            }

            MessageBox.Show("生成完成");


        }

        public Bitmap DrawIcon(string file,bool verteran)
        {
            SKImageInfo imageInfo = new SKImageInfo(60, 48);
            using (SKSurface surface = SKSurface.Create(imageInfo))
            {
                SKCanvas canvas = surface.Canvas;

                foreach (var obj in IconConfig.Template)
                {
                    switch (obj.Type)
                    {
                        case IconObjectType.Image:
                            {
                                using var input = File.OpenRead(obj.Src);
                                using var inputStream = new SKManagedStream(input);
                                using var bitmap = SKBitmap.Decode(inputStream);
                                canvas.DrawBitmap(bitmap, new SKPoint(obj.X, obj.Y));
                                break;
                            }
                        case IconObjectType.Text:
                            {
                                var paint = new SKPaint();
                                paint.TextAlign = SKTextAlign.Center;
                                paint.Color = string.IsNullOrEmpty(obj.TextColor)?  SKColor.Parse(IconConfig.TextColor): SKColor.Parse(obj.TextColor);
                                SKFontStyle fontStyle = new SKFontStyle(SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                                //SKTypeface sKTypeface = SKTypeface.FromFamilyName(IconConfig.FontFamily, fontStyle);
                                SKTypeface sKTypeface;
                                
                                if(!IconConfig.FontFamily.Contains("."))
                                {
                                    sKTypeface = SKTypeface.FromFamilyName(IconConfig.FontFamily);
                                }
                                else
                                {
                                    sKTypeface = SKTypeface.FromFile(IconConfig.FontFamily);
                                }
                                
                                

                                var skfont = new SKFont(sKTypeface, IconConfig.TextSize);
                                string name = Path.GetFileNameWithoutExtension(file);
                                string printText = name;
                                //if (IconConfig.TextFillSpace)
                                //{
                                //    if (printText.Count() >= 6)
                                //    {
                                //        printText = string.Join(" ", printText.ToList());
                                //    }
                                //    else
                                //    {
                                //        printText = string.Join(" ", printText.ToList());
                                //    }
                                //}

                                var textAll = SKTextBlob.Create(printText, skfont, new SKPoint(0, 0));

                                var texts = new List<SKTextBlob>();
                                foreach(var t in printText)
                                {
                                    texts.Add(SKTextBlob.Create(t.ToString(), skfont, new SKPoint(0, 0)));
                                }

                                var bound = new SKRect(0, 0, textAll.Bounds.Width / texts.Count(), textAll.Bounds.Height);
                                var spanX = (60 - texts.Count() * bound.Width) / (texts.Count() + 1);
                                var oleft = spanX;
                                //文字最小间距
                                if (spanX > 1.5)
                                {
                                    spanX = 1.5f;
                                    oleft = (60 - spanX * texts.Count() - bound.Width * texts.Count()) / 2;
                                }

                                oleft += obj.X;
                                
                                var top = obj.Y + ((60 - obj.Y) - bound.Height) / 2;

                                for (var i = 0; i < texts.Count(); i++)
                                {
                                    canvas.DrawText(texts[i], oleft + spanX * (i) + bound.Width * i, top, paint);
                                }

                                //var text = SKTextBlob.Create(printText, skfont, new SKPoint(0, 0));
                                //var left = (60 - text.Bounds.Width) / 2 - text.Bounds.Left;
                                //var top = obj.Y + ((60 - obj.Y) - text.Bounds.Height) / 2;
                                //canvas.DrawText(text, left, top, paint);
                                break;
                            }
                        case IconObjectType.Icon:
                            {
                                using var input = File.OpenRead(file);
                                using var inputStream = new SKManagedStream(input);
                                using var bitmap = SKBitmap.Decode(inputStream);
                                canvas.DrawBitmap(bitmap, new SKPoint(obj.X, obj.Y));
                                break;
                            }
                        case IconObjectType.Veteran:
                            {
                                if(verteran)
                                {
                                    using var input = File.OpenRead(IconConfig.VeteranIcon);
                                    using var inputStream = new SKManagedStream(input);
                                    using var bitmap = SKBitmap.Decode(inputStream);
                                    canvas.DrawBitmap(bitmap, new SKPoint(obj.X, obj.Y));
                                }
                                break;
                            }
                    }
                }
     
                using (SKImage image = surface.Snapshot())
                using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var mStream = new MemoryStream(data.ToArray()))
                {
                    Bitmap bm = new Bitmap(mStream, false);
                    return bm;
                }
            }
        }

        private List<string> files { get; set; } 

        private void btn_openfile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "图片 | *.png";
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if(openFileDialog1.FileNames.Count()>0)
                {
                    files = openFileDialog1.FileNames.ToList();
                    btn_openfile.Text = string.Join(",", files);
                }
            }
        }
    }

    public static class BitmapExtension
    {
        public static void SavePcxOrPng(this Bitmap bitmap,string fileName)
        {
            if(fileName.EndsWith(".png"))
            {
                bitmap.Save(fileName);
            }
            else
            {
                using var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Png);
                using MagickImage image = new MagickImage(stream.ToArray());
                {
                    image.Format = MagickFormat.Pcx;
                    image.ColorType = ColorType.Palette;
                    image.Write(fileName);
                }
            }
        }
    }
}