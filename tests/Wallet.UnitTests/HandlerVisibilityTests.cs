using ArchUnitNET.xUnitV3;
using KO.BuildingBlocks.Application.Abstraction.Messaging;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Wallet.UnitTests;

public class HandlerVisibilityTests : ArchitectureFixture
{
  [Fact]
  public void CommandHandlers_ShouldBeInternal()
  {
    Classes().That()
        .ImplementInterface(typeof(ICommandHandler<>))
        .Or()
        .ImplementInterface(typeof(ICommandHandler<,>))
        .Should().BeInternal()
        .Check(Architecture);
  }

  [Fact]
  public void QueryHandlers_ShouldBeInternal()
  {
    Classes().That()
        .ImplementInterface(typeof(IQueryHandler<,>))
        .Should().BeInternal()
        .Check(Architecture);
  }
}
