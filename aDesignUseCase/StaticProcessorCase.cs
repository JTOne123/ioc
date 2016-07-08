﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

#region Types & Classes used by the test cases in this TestClass

namespace App2.Contract
{
  public class RiskRequest
  {
    public int Threshold;
  }
  public class RiskResponse
  {
    public decimal RiskAmount { get; set; }
  }
}

namespace App2.BusinessLayer
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

  public static class ProcessorA
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
namespace App2.DataAccess
{
  public class Transit : BusinessLayer.ITransit
  {
    private decimal transit;
    public Transit() : this(transit: 1) { }
    public Transit(decimal transit)
    {
      this.transit = transit;
    }
    public decimal GetTransit() => transit;
  }
  public class Profile : BusinessLayer.IProfile
  {
    private decimal profile;
    public Profile() : this(profile: 2) { }
    public Profile(decimal profile)
    {
      this.profile = profile;
    }
    public decimal GetProfile() => profile;
  }
  public class Threshold : BusinessLayer.IThreshold
  {
    private decimal threshold;
    public Threshold() : this(threshold: 101) { }
    public Threshold(decimal threshold)
    {
      this.threshold = threshold;
    }
    public decimal GetLimit() => threshold;
  }
}

#endregion

namespace aDesignUseCase
{
  [TestClass]
  public class StaticProcessorCase
  {
    [TestMethod]
    public void ProcessorStaticOperation1()
    {
      //Arrange
      var typemap = new utility.TypeClassMapper(new Dictionary<string, object>
      {
        { "App2.BusinessLayer.ITransit", "App2.DataAccess.Transit, aDesignUseCase" },
        { "App2.BusinessLayer.IProfile", "App2.DataAccess.Profile, aDesignUseCase" },
        { "App2.BusinessLayer.IThreshold", "App2.DataAccess.Threshold, aDesignUseCase" }
      });
      var request = new App2.Contract.RiskRequest() { Threshold = 105 };

      //Act
      App2.Contract.RiskResponse response = App2.BusinessLayer.ProcessorA.GetRisk(request, typemap);

      //Assert
      Assert.AreEqual<decimal>(3, response.RiskAmount);
    }

    [TestMethod]
    public void ProcessorStaticOperation1WithInstances()
    {
      //Arrange
      var typemap = new utility.TypeClassMapper(new Dictionary<string, object>
      {
        { "App2.BusinessLayer.ITransit", new App2.DataAccess.Transit(100) },
        { "App2.BusinessLayer.IProfile", "App2.DataAccess.Profile, aDesignUseCase" },
        { "App2.BusinessLayer.IThreshold", new App2.DataAccess.Threshold(500) }
      });
      var request = new App2.Contract.RiskRequest() { Threshold = 501 };

      //Act
      App2.Contract.RiskResponse response = App2.BusinessLayer.ProcessorA.GetRisk(request, typemap);

      //Assert
      Assert.AreEqual<decimal>(102, response.RiskAmount);
    }
  }
}