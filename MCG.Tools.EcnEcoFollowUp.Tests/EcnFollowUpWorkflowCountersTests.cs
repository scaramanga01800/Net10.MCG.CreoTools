using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using Xunit;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    /// <summary>Semantique des compteurs workflow : trouve / changed / unchanged / notFound.</summary>
    public class EcnFollowUpWorkflowCountersTests
    {
        [Fact]
        public void Repository_Fake_UnknownEcn_IsNotFound()
        {
            var repo = new FakeEcnRepository();
            Assert.Equal(EcnUpdateOutcome.NotFound, repo.UpdateExistingEcn("X", _ => { }, CancellationToken.None));
        }

        [Fact]
        public void Repository_Fake_NoMutation_IsUnchanged()
        {
            var repo = new FakeEcnRepository();
            repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            Assert.Equal(EcnUpdateOutcome.Unchanged, repo.UpdateExistingEcn("B-1", _ => { }, CancellationToken.None));
        }

        [Fact]
        public void Repository_Fake_SeveralFieldsChanged_IsOneChangedRow()
        {
            var repo = new FakeEcnRepository();
            repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            var outcome = repo.UpdateExistingEcn("B-1", r => { r.EcnState = "Resolved"; r.NbPart = 3; }, CancellationToken.None);
            Assert.Equal(EcnUpdateOutcome.Changed, outcome);
        }

        [Fact]
        public void Workflow_FirstRunChanges_SecondRunIsUnchanged()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            h.AddChangeNotice("B-1");
            var first = h.Create().Synchronize();
            Assert.Equal(1, first.WorkflowEcnsFoundInWindchill);
            Assert.Equal(1, first.WorkflowEcnsChanged);
            Assert.Equal(0, first.WorkflowEcnsUnchanged);

            h.Repo.Rows[0].PdmUpdateStatus = h.Repo.Rows[0].PdmUpdateStatus; // meme etat persiste
            var second = h.Create().Synchronize();
            Assert.Equal(1, second.WorkflowEcnsFoundInWindchill);
            Assert.Equal(0, second.WorkflowEcnsChanged);
            Assert.Equal(1, second.WorkflowEcnsUnchanged);
        }

        [Fact]
        public void Workflow_NotFoundInWindchill_IsNeitherChangedNorUnchanged()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            var r = h.Create().Synchronize();
            Assert.Equal(1, r.WorkflowEcnsNotFoundInWindchill);
            Assert.Equal(0, r.WorkflowEcnsChanged);
            Assert.Equal(0, r.WorkflowEcnsUnchanged);
        }

        [Fact]
        public void Workflow_ChangedPlusUnchanged_EqualsFound()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 2, EcnNumber = "B-2" });
            h.AddChangeNotice("B-1"); h.AddChangeNotice("B-2");
            h.Create().Synchronize();
            var r = h.Create().Synchronize();
            Assert.Equal(r.WorkflowEcnsFoundInWindchill, r.WorkflowEcnsChanged + r.WorkflowEcnsUnchanged);
            Assert.Equal(2, r.WorkflowEcnsUnchanged);
        }
    }
}
