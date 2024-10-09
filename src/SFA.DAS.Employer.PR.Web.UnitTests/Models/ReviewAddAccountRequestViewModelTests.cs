using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;

public sealed class ReviewAddAccountRequestViewModelTests
{
    [Test]
    public void AcceptAddAccountRequestYesRadioCheck_ShouldReturnChecked_WhenAcceptAddAccountRequestIsTrue()
    {
        var viewModel = new ReviewAddAccountRequestSubmitViewModel
        {
            AcceptAddAccountRequest = true
        };

        var result = viewModel.AcceptAddAccountRequestYesRadioCheck;
        Assert.That("checked", Is.EqualTo(result));
    }

    [Test]
    public void AcceptAddAccountRequestYesRadioCheck_ShouldReturnEmpty_WhenAcceptAddAccountRequestIsFalse()
    {
        var viewModel = new ReviewAddAccountRequestSubmitViewModel
        {
            AcceptAddAccountRequest = false
        };
        var result = viewModel.AcceptAddAccountRequestYesRadioCheck;
        Assert.That(string.Empty, Is.EqualTo(result));
    }

    [Test]
    public void AcceptAddAccountRequestYesRadioCheck_ShouldReturnEmpty_WhenAcceptAddAccountRequestIsNull()
    {
        var viewModel = new ReviewAddAccountRequestSubmitViewModel
        {
            AcceptAddAccountRequest = null
        };
        var result = viewModel.AcceptAddAccountRequestYesRadioCheck;
        Assert.That(string.Empty, Is.EqualTo(result));
    }

    [Test]
    public void AcceptAddAccountRequestNoRadioCheck_ShouldReturnChecked_WhenAcceptAddAccountRequestIsFalse()
    {
        var viewModel = new ReviewAddAccountRequestSubmitViewModel
        {
            AcceptAddAccountRequest = false
        };
        var result = viewModel.AcceptAddAccountRequestNoRadioCheck;
        Assert.That("checked", Is.EqualTo(result));
    }

    [Test]
    public void AcceptAddAccountRequestNoRadioCheck_ShouldReturnEmpty_WhenAcceptAddAccountRequestIsTrue()
    {
        var viewModel = new ReviewAddAccountRequestSubmitViewModel
        {
            AcceptAddAccountRequest = true
        };
        var result = viewModel.AcceptAddAccountRequestNoRadioCheck;
        Assert.That(string.Empty, Is.EqualTo(result));
    }

    [Test]
    public void AcceptAddAccountRequestNoRadioCheck_ShouldReturnEmpty_WhenAcceptAddAccountRequestIsNull()
    {
        var viewModel = new ReviewAddAccountRequestSubmitViewModel
        {
            AcceptAddAccountRequest = null
        };
        var result = viewModel.AcceptAddAccountRequestNoRadioCheck;
        Assert.That(string.Empty, Is.EqualTo(result));
    }
}
