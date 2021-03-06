﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;
using MentorBot.Functions.Processors.Timesheets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    /// <summary>Tests for <see cref="RepeatProcessor" />.</summary>
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class OpenAirProcessorTests
    {
        private OpenAirProcessor _processor;
        private IOpenAirConnector _connector;
        private IMailService _mailService;
        private IStorageService _storageService;

        [TestInitialize]
        public void TestInitialize()
        {
            _connector = Substitute.For<IOpenAirConnector>();
            _storageService = Substitute.For<IStorageService>();
            _mailService = Substitute.For<IMailService>();
            _processor = new OpenAirProcessor(_connector, _storageService, _mailService);
        }

        [TestMethod]
        public void OpenAirProcessorSubjectShoudBeTimesheets()
        {
            Assert.AreEqual(_processor.Subject, "Timesheets");
        }

#pragma warning disable CS4014

        [TestMethod]
        public async Task OpenAirProcessor_ShouldReturnNoState()
        {
            var info = new TextDeconstructionInformation("Get unsubmitted timesheets", null);
            var accessor = Substitute.For<IPluginPropertiesAccessor>();

            accessor.GetAllPluginPropertyValues<string>(null).ReturnsForAnyArgs(new string[0]);

            // Act
            var result = await _processor.ProcessCommandAsync(info, CreateEvent("a@b.c"), null, accessor) as ChatEventResult;

            Assert.AreEqual("Provide a state of the time sheets, like unsubmitted or unapproved!", result.Text);
        }

        [TestMethod]
        public async Task WhenAskedItShouldGetTimesheets()
        {
            var chat = CreateEvent("a@b.c");
            var accessor = Substitute.For<IPluginPropertiesAccessor>();
            var responder = Substitute.For<IHangoutsChatConnector>();
            var timesheet = new Timesheet
            {
                UserName = "users/B",
                UserEmail = "c@d.e",
                DepartmentName = "F", Total = 20
            };

            var info = new TextDeconstructionInformation(
                "Get unsubmited timesheets",
                null,
                SentenceTypes.Unknown,
                new Dictionary<string, string[]> { { "State", new[] { "unsubmitted" } } },
                null,
                1.0);

            _connector.GetUnsubmittedTimesheetsAsync(DateTime.MinValue, new DateTime(2020, 1, 1), TimesheetStates.Unsubmitted, null, true, "OpenAir.User.MaxHours", new string[0]).ReturnsForAnyArgs(new[] { timesheet });

            // Act
            var result = await _processor.ProcessCommandAsync(info, chat, responder, accessor);

            // Test
            System.Threading.Thread.Sleep(150);

            Assert.AreEqual(null, result.Text);
            responder
                .Received()
                .SendMessageAsync(
                    null,
                    Arg.Is<GoogleChatAddress>(it => it.Sender == chat.Message.Sender && it.Space == chat.Space),
                    Arg.Any<Card[]>());
        }

        [TestMethod]
        public async Task WhenAskedNotifyForTimesheets()
        {
            var date = new DateTime(2000, 1, 1, 1, 1, 1);
            var customers = new[] { "D" };
            var responder = Substitute.For<IHangoutsChatConnector>();
            var address = new GoogleChatAddress("space/B", "MentorBot", null, "A", "Jhon");
            var timesheet = new Timesheet
            {
                UserName = "Jhon",
                UserEmail = "c@d.e",
                DepartmentName = "F",
                Total = 20
            };

            var timesheet2 = new Timesheet
            {
                UserName = "ElA",
                UserEmail = "w@n.m",
                DepartmentName = "F",
                Total = 15
            };

            responder.GetPrivateAddress(Arg.Any<IReadOnlyList<string>>()).Returns(new[] { address });

            _storageService.GetAddressesAsync().Returns(new GoogleAddress[0]);
            _connector.GetUnsubmittedTimesheetsAsync(date, date, TimesheetStates.Unsubmitted, "a@b.c", true, "OpenAir.User.MaxHours", null)
                .ReturnsForAnyArgs(new[] { timesheet, timesheet2 });

            // Act
            await _processor.NotifyAsync(
                new DateTime(2000, 1, 1, 1, 1, 1),
                TimesheetStates.Unsubmitted,
                "a@b.c",
                new[] { "D" },
                "F",
                true,
                true,
                true,
                null,
                responder);

            // Test
            responder.Received()
                .SendMessageAsync(
                    "Jhon, You have unsubmitted timesheet. Please, submit your timesheet.",
                    Arg.Is<GoogleChatAddress>(it => it.Space.Name == "space/B"));
            _mailService.Received()
                .SendMailAsync(
                    "Timesheet is pending",
                    ", You have unsubmitted timesheet. Please, submit your timesheet.",
                    Arg.Is<string[]>(it => it[0] == "w@n.m"));
            _storageService.Received()
                .AddAddressesAsync(Arg.Is<IReadOnlyList<GoogleAddress>>(arr => arr[0].SpaceName == "space/B"));
            _mailService.Received()
                .SendMailAsync("Users not notified", "All users with unsibmitted timesheets are notified! Total of 2.<br/><br/><b>The following people where notified by a direct massage or email:<br/><b>Jhon</b><br/><b>ElA</b>", "a@b.c");
        }

#pragma warning restore CS4014

        private static ChatEvent CreateEvent(string senderEmail)
        {
            var sender = new ChatEventMessageSender() { Email = senderEmail };
            var space = new ChatEventSpace();
            var message = new ChatEventMessage { Sender = sender };
            return new ChatEvent { Space = space, Message = message };
        }
    }
}
