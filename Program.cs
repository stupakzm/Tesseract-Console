using System.Net;
using Tesseract;

namespace TesseractTest {
    internal class Program {
        public static void Main(string[] args) {
            Console.WriteLine("Enter the image file path (or type 'exit' to quit): ");
            string input = Console.ReadLine();

            while (input.ToLower() != "exit") {
                string imagePath = input.Trim('"');

                if (!File.Exists(imagePath) && !Uri.IsWellFormedUriString(imagePath, UriKind.Absolute)) {
                    Console.WriteLine("Error: File not found or invalid URL: {0}", imagePath);
                    Console.WriteLine("Enter the image file path (or type 'exit' to quit): ");
                    input = Console.ReadLine();
                    continue;
                }

                try {
                    string projectPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string dataPath = Path.Combine(projectPath, @"tessdata_best-main");

                    if (!Directory.Exists(dataPath)) {
                        Console.WriteLine("Tesseract data files not found. Download them from https://github.com/tesseract-ocr/tessdata and place the 'tessdata_best-main' folder in: {0}", dataPath);
                        return;
                    }

                    using (var engine = new TesseractEngine(dataPath, "eng", EngineMode.Default)) {
                        Pix img;
                        if (Uri.IsWellFormedUriString(imagePath, UriKind.Absolute)) {
                            img = DownloadImageAsPix(imagePath);
                            if (img == null) {
                                Console.WriteLine("Error: Couldn't download image from URL.");
                                return;
                            }
                        }
                        else {
                            img = Pix.LoadFromFile(imagePath);
                        }

                        using (img) {
                            using (var page = engine.Process(img)) {
                                string text = page.GetText();
                                Console.WriteLine("Detected text:\n{0}", text);
                                Console.ReadKey();
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Error: {0}", ex.Message);
                }

                Console.WriteLine("Enter the image file path (or type 'exit' to quit): ");
                input = Console.ReadLine();
            }

            Console.WriteLine("Exiting program...");
        }

        private static Pix DownloadImageAsPix(string imageUrl) {
            try {
                using (var webClient = new WebClient()) {
                    using (var memoryStream = new MemoryStream(webClient.DownloadData(imageUrl))) {
                        return Pix.LoadFromMemory(memoryStream.ToArray());
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Error downloading image: {0}", ex.Message);
                return null;
            }
        }
    }
}
