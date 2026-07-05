using ArchUnitNET.Domain;
using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Wallet.UnitTests;

public class LayerTests : ArchitectureFixture
{
  private static readonly IObjectProvider<IType> DomainLayer =
    Types().That().ResideInAssembly(ArchitectureFixture.DomainAssembly)
           .As("Domain Layer");

  private static readonly IObjectProvider<IType> ApplicationLayer =
      Types().That().ResideInAssembly(ArchitectureFixture.ApplicationAssembly)
             .As("Application Layer");

  private static readonly IObjectProvider<IType> InfrastructureLayer =
      Types().That().ResideInAssembly(ArchitectureFixture.InfrastructureAssembly)
             .As("Infrastructure Layer");

  private static readonly IObjectProvider<IType> PresentationLayer =
      Types().That().ResideInAssembly(ArchitectureFixture.PresentationAssembly)
             .As("Presentation Layer");

  [Fact]
  public void DomainLayer_ShouldNotDependOn_ApplicationLayer()
  {
    var rule = Types().That().Are(DomainLayer)
        .Should().NotDependOnAny(ApplicationLayer);

    rule.Check(Architecture);
  }

  [Fact]
  public void DomainLayer_ShouldNotDependOn_InfrastructureLayer()
  {
    var rule = Types().That().Are(DomainLayer)
        .Should().NotDependOnAny(InfrastructureLayer);

    rule.Check(Architecture);
  }

  [Fact]
  public void DomainLayer_ShouldNotDependOn_PresentationLayer()
  {
    var rule = Types().That().Are(DomainLayer)
        .Should().NotDependOnAny(PresentationLayer);

    rule.Check(Architecture);
  }

  [Fact]
  public void ApplicationLayer_ShouldNotDependOn_InfrastructureLayer()
  {
    var rule = Types().That().Are(ApplicationLayer)
        .Should().NotDependOnAny(InfrastructureLayer);

    rule.Check(Architecture);
  }

  [Fact]
  public void ApplicationLayer_ShouldNotDependOn_PresentationLayer()
  {
    var rule = Types().That().Are(ApplicationLayer)
        .Should().NotDependOnAny(PresentationLayer);

    rule.Check(Architecture);
  }

  [Fact]
  public void InfrastructureLayer_ShouldNotDependOn_PresentationLayer()
  {
    var rule = Types().That().Are(InfrastructureLayer)
        .Should().NotDependOnAny(PresentationLayer);

    rule.Check(Architecture);
  }
}