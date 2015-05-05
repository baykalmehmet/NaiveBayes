using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Classifier
{
  /// <summary>
  /// This is Naive Bayes Training wrapper
  /// </summary>
  class NaiveBayesWrapper
  {
    /// <summary>
    /// This is address of the labelled data
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// This is where output file will be saved
    /// after processing documents
    /// </summary>
    public string OutputFolder { get; private set; }

    /// <summary>
    /// Acitvate parameter smoothing for large documents set.
    /// </summary>
    public bool ParamSmoothing { get; private set; }

    /// <summary>
    /// Initialisers
    /// </summary>
    /// <param name="docsLocation">Where document collections are stored</param>
    /// <param name="Output">file location of the processes documebts</param>
    /// <param name="Smoothing">Use parameter smoothing for large documents set in order to have noin zero prediction results</param>
    public NaiveBayesWrapper(string docsLocation,string Output, bool Smoothing)
    {
      this.FilePath = docsLocation;
      this.OutputFolder = Output;
    }

    /// <summary>
    /// This class takes text as input and returns prediction object
    /// </summary>
    /// <param name="t"></param>
    /// <param name="Document"></param>
    /// <returns></returns>
    public Prediction Predict(NaiveBayes t, string Document)
    {
      Prediction ret = null;
      PredictionWorker(t, Document, ret);
      return ret;
    }

    /// <summary>
    /// this is locate naive bayes train file and starts classification
    /// </summary>
    /// <param name="NaiveBayesLocation">training file location</param>
    /// <param name="TrainFileName">train file location</param>
    /// <param name="Document">documents need to be classified</param>
    /// <returns></returns>
    public Prediction Predict(string NaiveBayesLocation, string TrainFileName, string Document)
    {
      Prediction ret = null;
      NaiveBayes t = LoadTrainer(NaiveBayesLocation, TrainFileName); ;
      if (t!=null)
      {
        PredictionWorker(t, Document, ret);
      }
      return ret;
    }

    /// <summary>
    /// This is load training file to memory
    /// </summary>
    /// <param name="NaiveBayesLocation">location of the naive bayes train file</param>
    /// <param name="TrainFileName">file name of the trainign data</param>
    /// <returns></returns>
    public NaiveBayes LoadTrainer(string NaiveBayesLocation, string TrainFileName)
    {
      NaiveBayes t = null;
      string _filePath = string.Concat(NaiveBayesLocation, @"\", TrainFileName);
      if (File.Exists(_filePath))
      {
        using (Stream sr = File.OpenRead(_filePath))
        {
          BinaryFormatter f = new BinaryFormatter();
          t = (NaiveBayes)f.Deserialize(sr);
        }
      }
      return t;
    }

    /// <summary>
    /// refactored method
    /// </summary>
    /// <param name="t"></param>
    /// <param name="Document"></param>
    /// <param name="ret"></param>
    private  void PredictionWorker(NaiveBayes t, string Document, Prediction ret)
    {
      TextEngine engine = new TextEngine(Document);
      List<string> ngram = engine.getNGrams();
      Dictionary<string, float> scoreBoard = new Dictionary<string, float>();
      foreach (var label in t.TrainingLabels)
      {
        float priori = label.Value;
        string strLabel = label.Key;
        List<float> score = new List<float>();
        foreach (var word in ngram)
        {
          //get total value
          if (t.ClassifiedText.Exists(x => x.strWord == word && x.Labels.ContainsKey(strLabel)))
          {
            var item = t.ClassifiedText.Where(x => x.strWord == word && x.Labels.ContainsKey(strLabel)).FirstOrDefault();
            score.Add(item.Probability[strLabel]);
          }
        }

        float totalScore = score[0];//re factor the values
        for (int i = 1; i < score.Count; i++)
        {
          totalScore *= (float)score[i];

        }
        totalScore = totalScore * (float)t.TrainingLabels[strLabel];
        scoreBoard.Add(strLabel, totalScore);
      }
      if (scoreBoard.Count > 0)
      {
        float maxScore = scoreBoard.Max(h => h.Value);
        var _score = scoreBoard.Where(p => p.Value == maxScore).First();
        ret.Label = _score.Key;
        ret.Score = _score.Value;
      }
    }
    public void Train()
    {
      Dictionary<string, List<string>> TrainingSet = new Dictionary<string, List<string>>();
      if (!Directory.Exists(this.OutputFolder))
      {
        Directory.CreateDirectory(this.OutputFolder);
      }
      if (Directory.Exists(this.FilePath))
      {
        DirectoryInfo dirInfo = new DirectoryInfo(this.FilePath);
        foreach (var _file in dirInfo.GetFiles())
        {
          var name = _file.Name.Replace(_file.Extension, "");
          List<string> Documents = new List<string>();
          foreach (var line in System.IO.File.ReadLines(_file.FullName))
          {
            Documents.Add(line);
          }
          TrainingSet.Add(name, Documents);
        }
        NaiveBayes _naiveBayes = new NaiveBayes(TrainingSet) { ParamSmoothing = this.ParamSmoothing };
        _naiveBayes.Train();
        //export training file now
        using (FileStream _fs = File.Create(string.Concat(this.OutputFolder, @"\job_classifier.jc")))
        {
          BinaryFormatter _binForm = new BinaryFormatter();
          _binForm.Serialize(_fs, _naiveBayes);
          _fs.Flush();
          _fs.Close();
        }
      }
      else
      {
        throw new DirectoryNotFoundException();
      }
    }
  }
  /// <summary>
  /// this is for naive bayes prediction
  /// </summary>
  public class Prediction
  {
    public float Score { get; set; }
    public string Label { get; set; }
  }

}
