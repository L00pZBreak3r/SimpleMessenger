// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const mClientInfo = {
        ServerUrl: "",
        UserId:    0,
        UserName:  "",
        IsGenerator: false,
        GeneratorIntervalId: 0,
        WebSocketInstance: null
    };
const cPostHeaders = {
    "Content-Type": "application/json;charset=utf-8"
};

function StopClient()
{
    if (mClientInfo.IsGenerator)
    {
        clearInterval(mClientInfo.GeneratorIntervalId);
        mClientInfo.IsGenerator = false;
        mClientInfo.GeneratorIntervalId = 0;
    }
    if (mClientInfo.WebSocketInstance)
    {
        mClientInfo.WebSocketInstance.close();
        mClientInfo.WebSocketInstance = null;
    }
}

function ResetClientInfo()
{
    StopClient();
    mClientInfo.ServerUrl = "";
    mClientInfo.UserId = 0;
    mClientInfo.UserName = "";
}

function DoConnect()
{
    ResetClientInfo();

    const aServerUrl = document.getElementById("mServerUrl").value;
    const aDivUserList = document.getElementById("mUserList");

    fetch('https://' + aServerUrl + '/v1/getusers')
      .then((response) => response.json())
      .then((result) => {
        mClientInfo.ServerUrl = aServerUrl;
        const aUsers = result.Users;
        aDivUserList.innerText = "Выберите пользователя: " + aUsers.map(u => u.Name).join();
      });
}

async function GetUserId(aUserName)
{
    let aUserId = 0;
    if (mClientInfo.ServerUrl && aUserName)
    {
        const aResponse = await fetch('https://' + mClientInfo.ServerUrl + '/v1/getuserid', {
                method: "POST",
                headers: cPostHeaders,
                body: JSON.stringify(aUserName),
                signal: AbortSignal.timeout(5000)
              });
        if (aResponse.ok)
        {
            const aText = await aResponse.text();
            if (aText)
                aUserId = Number(aText);
        }
    }
    return aUserId;
}

function CreateMessageDiv(aMessage)
{
    const aMessageContentDivClass = "w-75" + ((aMessage.IsMine) ? " message-mine ms-5" : " message-other");
    const aUserTitle = (aMessage.IsMine) ? "To: " : "From: ";

    const aMessageDiv = document.createElement("div");
    aMessageDiv.className = "mb-3";

    const aMessageUserNameDiv = document.createElement("div");
    aMessageUserNameDiv.textContent = aUserTitle + aMessage.OtherUserName;
    aMessageUserNameDiv.className = aMessageContentDivClass;

    const aMessageInfoDiv = document.createElement("div");
    aMessageInfoDiv.textContent = `${aMessage.Number} [${aMessage.TimeStamp}]`;
    aMessageInfoDiv.className = aMessageContentDivClass;

    const aMessageContentDiv = document.createElement("div");
    aMessageContentDiv.textContent = aMessage.Text;
    aMessageContentDiv.className = aMessageContentDivClass;

    aMessageDiv.appendChild(aMessageUserNameDiv);
    aMessageDiv.appendChild(aMessageInfoDiv);
    aMessageDiv.appendChild(aMessageContentDiv);

    return aMessageDiv;
}

function FillDivMessages(aMessages)
{
    const aDivMessages = document.getElementById("mDivMessages");
    aDivMessages.textContent = '';

    aMessages.forEach((aMessage) => {
      aMessage.TimeStamp = new Date(aMessage.TimeStamp);
      const aDiv = CreateMessageDiv(aMessage);
      aDivMessages.appendChild(aDiv);
    });
}

function AddDivMessages(aMessage)
{
    const aDivMessages = document.getElementById("mDivMessages");

    const aDiv = CreateMessageDiv(aMessage);
    aDivMessages.appendChild(aDiv);
}

function WebSocketSendUserId()
{
    if (mClientInfo.WebSocketInstance && mClientInfo.WebSocketInstance.readyState === WebSocket.OPEN)
        mClientInfo.WebSocketInstance.send(mClientInfo.UserId.toString());
}

function ProcessWebSocketOpen(aEvent)
{
    WebSocketSendUserId();
}

function ProcessWebSocketMessage(aEvent)
{
    const aNewTextMessage = JSON.parse(aEvent.data);
    aNewTextMessage.TimeStamp = new Date(aNewTextMessage.TimeStamp.Seconds * 1000 + aNewTextMessage.TimeStamp.Nanos / 1000000);
    WebSocketSendUserId();
    AddDivMessages(aNewTextMessage);
}

function GetMessagesRange(aStart, aEnd)
{
    if (mClientInfo.ServerUrl && mClientInfo.UserId)
    {
        const aBody = {
            UserId: mClientInfo.UserId,
            Start: aStart.toJSON(),
            End: aEnd.toJSON()
        };

        fetch('https://' + mClientInfo.ServerUrl + '/v1/gettextmessagesfromrange', {
                method: "POST",
                headers: cPostHeaders,
                body: JSON.stringify(aBody),
                signal: AbortSignal.timeout(5000)
              })
          .then((response) => response.json())
          .then((result) => {
            FillDivMessages(result.Messages);
          });
    }
}

function DoGetMessages()
{
    if (mClientInfo.ServerUrl && mClientInfo.UserId)
    {
        const aStart = new Date(0);
        const aEnd = new Date();
        aEnd.setMinutes(aEnd.getMinutes() + 1);

        GetMessagesRange(aStart, aEnd);
    }
}

async function DoLogin()
{
    StopClient();
    if (mClientInfo.ServerUrl)
    {
        const aUserName = document.getElementById("mUserName").value;
        const aUserId = await GetUserId(aUserName);
        if (aUserId)
        {
            const aDivUserInfo = document.getElementById("mUserInfo");

            mClientInfo.UserId = aUserId;
            mClientInfo.UserName = aUserName;
            aDivUserInfo.innerText = "User id: " + aUserId;
            DoGetMessages();

            mClientInfo.WebSocketInstance = new WebSocket("wss://" + mClientInfo.ServerUrl + "/ws");
            mClientInfo.WebSocketInstance.addEventListener("open", ProcessWebSocketOpen);
            mClientInfo.WebSocketInstance.addEventListener("message", ProcessWebSocketMessage);
        }
    }
}

function isPositiveInteger(n)
{
    return n >>> 0 === parseFloat(n);
}

function DoGetHistory()
{
    if (mClientInfo.ServerUrl && mClientInfo.UserId)
    {
        const aStartDateString = document.getElementById("mStartDate").value;
        const aEndDateString = document.getElementById("mEndDate").value;
        let aStart = new Date();
        if (aStartDateString)
        {
            if (isPositiveInteger(aStartDateString))
                aStart.setMinutes(aStart.getMinutes() - Number(aStartDateString));
            else
                aStart = new Date(aStartDateString);
        }
        else
            aStart = new Date(0);
        let aEnd = new Date();
        if (aEndDateString)
        {
            if (isPositiveInteger(aEndDateString))
                aEnd.setMinutes(aEnd.getMinutes() - Number(aEndDateString));
            else
                aEnd = new Date(aEndDateString);
        }
        else
            aEnd.setMinutes(aEnd.getMinutes() + 1);

        GetMessagesRange(aStart, aEnd);
    }
}

function GetRandomText()
{
    let aResult = "";
    for (let i = 0; i < 128; i++)
    {
        aResult += Math.random().toString(36) + " ";
    }
    return aResult;
}

async function SendMessageGen(aBody, aDivSendError, aIsGen)
{
    if (mClientInfo.ServerUrl && mClientInfo.UserId)
    {
        const aAbortTimeout = (aIsGen) ? 800 : 5000;
        if (aIsGen)
            aBody.Text = GetRandomText();

        const response = await fetch('https://' + mClientInfo.ServerUrl + '/v1/sendtextmessage', {
                method: "POST",
                headers: cPostHeaders,
                body: JSON.stringify(aBody),
                signal: AbortSignal.timeout(aAbortTimeout)
              });
        if (response.ok)
        {
            let result = await response.json();
            if (result)
            {
                if (aIsGen)
                    aBody.Number++;
                aDivSendError.className = (result.Error) ? "bg-danger-subtle" : "bg-success-subtle";
                aDivSendError.textContent = (result.Error) ? ("Ошибка: " + result.Error) : ("Отправлено: " + result.TimeStamp);
            }
        }
    }
}

async function DoSendMessage()
{
    const aSendError = document.getElementById("mSendError");
    aSendError.textContent = '';

    if (mClientInfo.ServerUrl && mClientInfo.UserId)
    {
        const aMessageText = document.getElementById("mMessageText").value;
        const aReceiverName = document.getElementById("mReceiverName").value;
        const aMessageNumber = document.getElementById("mMessageNumber").value;
        if (aMessageText && aReceiverName && isPositiveInteger(aMessageNumber))
        {
            const aReceiverId = await GetUserId(aReceiverName);
            if (aReceiverId)
            {
                const aBody = {
                    SenderId: mClientInfo.UserId,
                    ReceiverId: aReceiverId,
                    Text: aMessageText,
                    Number: Number(aMessageNumber)
                };
                await SendMessageGen(aBody, aSendError, false);
            }
        }
    }
}

async function DoStartGenerator()
{
    const aGeneratorButton = document.getElementById("mStartGeneratorButton");
    if (mClientInfo.IsGenerator)
    {
        clearInterval(mClientInfo.GeneratorIntervalId);
        mClientInfo.IsGenerator = false;
        mClientInfo.GeneratorIntervalId = 0;
        aGeneratorButton.textContent = "Включить";
        aGeneratorButton.className = "btn btn-primary";
    }
    else if (mClientInfo.ServerUrl && mClientInfo.UserId)
    {
        const aReceiverName = document.getElementById("mReceiverName").value;
        const aMessageNumber = document.getElementById("mMessageNumber").value;
        const aGeneratorInterval = document.getElementById("mGeneratorInterval").value;
        if (aReceiverName && isPositiveInteger(aMessageNumber) && isPositiveInteger(aGeneratorInterval))
        {
            const aReceiverId = await GetUserId(aReceiverName);
            if (aReceiverId)
            {
                const aInterval = Number(aGeneratorInterval);
                mClientInfo.IsGenerator = true;

                const aSendError = document.getElementById("mSendError");
                const aBody = {
                    SenderId: mClientInfo.UserId,
                    ReceiverId: aReceiverId,
                    Text: "",
                    Number: Number(aMessageNumber)
                };
                mClientInfo.GeneratorIntervalId = setInterval(SendMessageGen, aInterval * 1000, aBody, aSendError, true);
                aGeneratorButton.textContent = "Выключить";
                aGeneratorButton.className = "btn btn-danger";
            }
        }
    }
}
