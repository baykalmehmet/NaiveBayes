using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Classifier
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("please use train or classify output:<path>");
      Console.WriteLine(@"train E:\projects\naive-bayes-train");
      //string path = @"E:\projects\naive-bayes-train";
      string path = @"C:\test"; //args[1];
      //////if (command.ToLower() == "train")
      //////{
      //Dictionary<string, List<string>> TrainingSet = new Dictionary<string, List<string>>();
      //System.IO.DirectoryInfo dr = new System.IO.DirectoryInfo(path);
      //foreach (var item in dr.GetFiles())
      //{
      //  var name = item.Name.Replace(item.Extension, "");
      //  List<string> documents = new List<string>();
      //  foreach (var line in System.IO.File.ReadLines(item.FullName))
      //  {
      //    documents.Add(line);
      //  }
      //  TrainingSet.Add(name, documents);
      //}
      //NaiveBayes nb = new NaiveBayes(TrainingSet) { ParamSmoothing=true};
      //nb.Train();
      //using (FileStream fs = File.Create(string.Concat(path, @"\trainin_data_set.dat")))
      //{
      //  BinaryFormatter bf = new BinaryFormatter();
      //  bf.Serialize(fs, nb);
      //  fs.Flush();
      //  fs.Close();
      //}
      
      ////else
      ////{
      Console.WriteLine("loading training-data...");

      //read
      NaiveBayes t = null;
      using (Stream sr = File.OpenRead(string.Concat(path, @"\trainin_data_set.dat")))
      {
        BinaryFormatter f = new BinaryFormatter();
        t = (NaiveBayes)f.Deserialize(sr);
      }
      Console.WriteLine("trainign data is loaded");
      string text = File.ReadAllText("text.txt");
      TextEngine engine = new TextEngine(text);
      List<string> ngram = engine.getNGrams();
      Dictionary<string, float> scoreBoard = new Dictionary<string, float>();
      //calculate each word for each ngram
      foreach (var label in t.TrainingLabels)
      {
        float priori = label.Value;
        string strLabel = label.Key;
        List<float> score = new List<float>();
        foreach (var word in ngram)
        {
          //get total value
        //  score.Add(t.ClassifiedText.Find.Where(x=>x.strWord==word && x.Probability.ContainsKey(strLabel));
          if (t.ClassifiedText.Exists(x=>x.strWord==word && x.Labels.ContainsKey(strLabel)))
          {
            var item = t.ClassifiedText.Where(x => x.strWord == word && x.Labels.ContainsKey(strLabel)).FirstOrDefault();
            score.Add(item.Probability[strLabel]);
          }
        }

        float totalScore = score[0];//re factor the values
        for (int i = 1  ; i < score.Count; i++)
        {
          totalScore *= (float)score[i];
        
        }
        totalScore = totalScore * (float)t.TrainingLabels[strLabel];
        scoreBoard.Add(strLabel, totalScore);
      }

      //print scores
      Console.WriteLine("\n\nScore Board...");
      float maxScore=scoreBoard.Max(h=>h.Value);
      var ret = scoreBoard.Where(p => p.Value == maxScore).First();
      Console.WriteLine("score:{0:F10} -- {1}", ret.Value,ret.Key);
      
      Console.Read();
      //}
    }
  }
}
