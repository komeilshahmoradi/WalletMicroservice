using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
namespace Wallet.UnitTests;

public class DependencyGuardTests : ArchitectureFixture
{
  [Fact]
  public void DomainLayer_ShouldNotDependOn_EntityFramework()
  {
    Types().That().ResideInAssembly(DomainAssembly).Should()
        .NotDependOnAnyTypesThat()
        .ResideInNamespace("Microsoft.EntityFrameworkCore")
        .Check(Architecture);
  }
}
