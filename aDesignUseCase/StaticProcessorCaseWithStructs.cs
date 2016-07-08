﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

#region Types & Classes used by the test cases in this TestClass

namespace App3.Contract
{
  public struct RiskRequest
  {
    public int Threshold;
  }
  public struct RiskResponse
  {
    public decimal RiskAmount { get; set; }
  }
}

namespace App3.BusinessLayer
{
  public interface ITransit
  {
    decimal GetTransit();
  }
  public interface IProfile
  {
    decimal GetProfile();
  }
  public interface IThreshold
  {
    decimal GetLimit();
  }

  public static class ProcessorB
  {
    public static Contract.RiskResponse GetRisk(Contract.RiskRequest request, IServiceProvider typemap)
    {
      var transit = (ITransit)typemap.GetService(typeof(ITransit));
      var profile = (IProfile)typemap.GetService(typeof(IProfile));
      var threshold = (IThreshold)typemap.GetService(typeof(IThreshold));

      var response = new Contract.RiskResponse();
      response.RiskAmount = request.Threshold > threshold.GetLimit() ? transit.GetTransit() + profile.GetProfile() : 0;
      return response;
    }
  }
}
namespace App3.DataAccess
{
  public struct Transit : BusinessLayer.ITransit
  {
    private decimal? transit;
    public Transit(decimal transit)
    {
      this.transit = transit;
    }
    public decimal GetTransit() => transit ?? GetLoadedTransit();
    private decimal GetLoadedTransit() { return 1; }
  }
  public struct Profile : BusinessLayer.IProfile
  {
    private decimal? profile;
    public Profile(decimal profile)
    {
      this.profile = profile;
    }
    public decimal GetProfile() => profile ?? GetLoadedProfile();
    private decimal GetLoadedProfile() { return 2; }
  }
  public struct Threshold : BusinessLayer.IThreshold
  {
    private decimal? threshold;
    public Threshold(decimal threshold)
    {
      this.threshold = threshold;
    }
    public decimal GetLimit() => threshold ?? GetLoadedProfile();
    private decimal GetLoadedProfile() { return 101; }
  }
}

#endregion

namespace aDesignUseCase
{
  [TestClass]
  public class StaticProcessorCaseWithStructs
  {
    [TestMethod]
    public void ProcessorStaticOperation1()
    {
      //Arrange
      var typemap = new utility.TypeClassMapper(new Dictionary<string, object>
      {
        { "App3.BusinessLayer.ITransit", "App3.DataAccess.Transit, aDesignUseCase" },
        { "App3.BusinessLayer.IProfile", "App3.DataAccess.Profile, aDesignUseCase" },
        { "App3.BusinessLayer.IThreshold", "App3.DataAccess.Threshold, aDesignUseCase" }
      });
      var request = new App3.Contract.RiskRequest() { Threshold = 105 };

      //Act
      App3.Contract.RiskResponse response = App3.BusinessLayer.ProcessorB.GetRisk(request, typemap);

      //Assert
      Assert.AreEqual<decimal>(3, response.RiskAmount);
    }

    [TestMethod]
    public void ProcessorStaticOperation1WithInstances()
    {
      //Arrange
      var typemap = new utility.TypeClassMapper(new Dictionary<string, object>
      {
        { "App3.BusinessLayer.ITransit", new App3.DataAccess.Transit(100) },
        { "App3.BusinessLayer.IProfile", "App3.DataAccess.Profile, aDesignUseCase" },
        { "App3.BusinessLayer.IThreshold", new App3.DataAccess.Threshold(500) }
      });
      var request = new App3.Contract.RiskRequest() { Threshold = 501 };

      //Act
      App3.Contract.RiskResponse response = App3.BusinessLayer.ProcessorB.GetRisk(request, typemap);

      //Assert
      Assert.AreEqual<decimal>(102, response.RiskAmount);
    }
  }
}