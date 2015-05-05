using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Classifier
{
  [Serializable()]
  [XmlInclude(typeof(Keyword))]
  public class NaiveBayes
  {
    /// <summary>
    /// trainign dataset
    /// </summary>
    public Dictionary<string,List<string>> TrainingSet { get;   set; }
    /// <summary>
    /// this container is for label vs keyword allocation
    /// </summary>
    public List<Keyword>  ClassifiedText { get;  set; }
    public Dictionary<string,int> LabelSize { get;  set; }

    public Dictionary<string, float> TrainingLabels { get; set; }


    public bool ParamSmoothing { get; set; }

    public NaiveBayes(Dictionary<string, List<string>> tDataSet)
    {
      this.TrainingSet = tDataSet;
      this.ClassifiedText = new List<Keyword>();
      this.TrainingLabels = new Dictionary<string, float>();
      this.LabelSize = new Dictionary<string, int>();
    }


    public NaiveBayes() { }

    /// <summary>
    /// This is responsible for priori calculation
    /// </summary>
    private void calculatePriori()
    { 
    int cntDoco=this.TrainingSet.Sum(t=>t.Value.Count());
    foreach (var label in this.TrainingSet)
    {
      this.TrainingLabels[label.Key]= (float)label.Value.Count / (float)cntDoco;
    }
    ///clear the trainign set to make data smaller
    this.TrainingSet.Clear();
    }


    private void CalculateWordScore()
    {
      RemoveNoise();
      int totalUniqueWord = this.ClassifiedText.Count();   //ignore the label which has single cat
      //update the label size
      foreach (var lbl in this.ClassifiedText)  //ignore sinlge counts as noise
      {
        foreach (var txt in lbl.Labels)
        {
          this.LabelSize[txt.Key] += txt.Value;
        }
      }
      foreach (var item in this.ClassifiedText)
      {
        foreach (var label in item.Labels)
        {
          ///(number of word occurances given the class+1)/(total word  in the specific labels)+(total unique word in all categories)
          ///
          float probability = (((float)(label.Value + 1)) / ((float)this.LabelSize[label.Key] + (float)totalUniqueWord));
          probability=this.ParamSmoothing?probability+1F:probability;
          item.Probability.Add(label.Key, probability);
        }
      }
    }

    private void RemoveNoise()
    {
      var noise = this.ClassifiedText.Where(p => p.Labels.Values.Max() <= 1 && p.Labels.Keys.Count <= 1).ToList();
      foreach (var item in noise)
      {
        this.ClassifiedText.Remove(item);
      }
    }


    private void StripURLs() { throw new NotImplementedException(); }
    private void StripEmails() { throw new NotImplementedException(); }



    private void CalculateStatistics()
    { 
    
    }


    public void Train()
    { 
    if (this.TrainingSet.Count>0)
      {
        Process();
        calculatePriori();
        CalculateWordScore();
        RemoveNoise();
      }
    }


    private void Process()
    {
      TextEngine tEng=new TextEngine();
      foreach (var item in TrainingSet)
      {
        Console.WriteLine("label is being processed: {0}", item.Key);
        //update labels
        if (!this.TrainingLabels.ContainsKey(item.Key))
        {
               this.TrainingLabels.Add(item.Key,0.0F);
               this.LabelSize.Add(item.Key, 0);
        }
   
        foreach (var strLine in item.Value)
        {
          tEng.Text2BNormalized = strLine;
          var cleansedText = tEng.Process();
          foreach (var keyword in tEng.getNGrams())
          {
            if (this.ClassifiedText.Exists(t => t.strWord == keyword))
            {
              var element = this.ClassifiedText.Where(t => t.strWord == keyword).FirstOrDefault();
              //label is not exists
              if (element.Labels.ContainsKey(item.Key))
              {
                element.Labels[item.Key] += 1;  //update the counter
              }
              else
              {
                element.Labels.Add(item.Key, 1); //add the label
              }
              //label exists
            }
            else
            {
              Keyword w = new Keyword();
              w.strWord = keyword;
              w.Labels.Add(item.Key, 1);
              this.ClassifiedText.Add(w);
            }
          }
        }
      }
    }



  }

  /// <summary>
  /// this is to store keyword 
  /// </summary>
  /// 
    [Serializable()]
  public class Keyword
  {
    public string strWord { get; set; }
    //count
    public Dictionary<string,int> Labels { get; set; }
    /// <summary>
    /// this will contain each word score based on the given label
    /// </summary>
    public Dictionary<string, float> Probability { get; set; }
    public Keyword()
    {
      this.Labels = new Dictionary<string, int>();
      this.Probability = new Dictionary<string, float>();
    }
  }



}
