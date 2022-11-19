using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using SimpleHttp;
using System.Threading;
using NLog;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using COD_Discord_Bot.Responses;
using System.Runtime.ExceptionServices;
using COD_Discord_Bot;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Converters;
using COD_Discord_Bot.Utils;
using Emzi0767;
using Emzi0767.Utilities;
using COD_Discord_Bot.Shoppy;
using DSharpPlus.Net;
using System.Net;

class ShoppyWebhooks {
    private DiscordGuild Client { get; set; }
    private int Port { get; set; }
    private CancellationTokenSource Token = new CancellationTokenSource();
    private readonly Logger logger = LogManager.GetCurrentClassLogger();
    private Shoppy shoppy = Configuration.Shoppys[(ulong)ServerIds.CodAccounts];
    public ShoppyWebhooks(DiscordGuild client, int port) {
        this.Client = client;
        this.Port = port;
    }

    public async void Start() {
        logger.Debug($"Trying to start webhook listener on port {Port}");
        await RegisterOnOrder();
        await RegisterDamsSold();
        new Thread(() => {
            HttpServer.ListenAsync(8080, Token.Token, Route.OnHttpRequestAsync).Wait();
        }).Start();
        
        logger.Debug($"Webhooked startd on port {Port}");
    }

    private Task RegisterDamsSold() {
        Route.Add("/damsSold", async (req, res, props) => {
            var encoding = req.ContentEncoding;
            var reader = new System.IO.StreamReader(req.InputStream, req.ContentEncoding);
            var response = await reader.ReadToEndAsync();
            var json = JObject.Parse(response);
            try {
                if ((bool)json["status"]) {
                    var order = await shoppy.GetOrder((string)json["data"]["order"]["id"]);
                    if (order.Success) {
                        var channel = this.Client.GetChannel((ulong)ChannelIds.saleLog);
                        await channel.SendMessageAsync($"{order.ItemName} ({(int)order.PricePaid}$) - {order.Email}");
                    }
                } else logger.Error($"Couldn't read webhook {response}");
            } catch (NullReferenceException) {
                logger.Error($"No order id for {response}???");
            } catch (Exception ex) {
                logger.Error($"Couldn't read order. Error: {ex.ToString()}");
            } finally {
                res.StatusCode = (int)HttpStatusCode.OK;
                res.AsText("all good");
            }

        }, "POST");
        return Task.CompletedTask;
    }

    private Task RegisterOnOrder() {
        Route.Add("/onOrder", async (req, res, props) => {
            var encoding = req.ContentEncoding;
            var reader = new System.IO.StreamReader(req.InputStream, req.ContentEncoding);
            var response = await reader.ReadToEndAsync();
            var json = JObject.Parse(response);
            try {
                if ((bool)json["status"]) {
                    var order = await shoppy.GetOrder((string)json["data"]["order"]["id"]);
                    if (order.BoughtCount > 0) {
                        if (order.DiscordID != default) {
                            if (order.DiscordID.CalculateLength() == 18) {
                                logger.Info($"Trying to send order with email {order.Email} to id {order.DiscordID}");
                                var o = await SendOrder(order, Client);
                                if (o) logger.Info($"Sent order to with email {order.Email} to id {order.DiscordID}");
                                else logger.Error($"User with id {order.DiscordID} email {order.Email} not in my server???");
                            } else logger.Error($"Wrong discord id??? {order.DiscordID}");
                        }
                    }
                } else logger.Error($"Couldn't read webhook {response}");
            } catch (NullReferenceException) {
                logger.Error($"No order id for {response}???");
            } catch (Exception ex) {
                logger.Error($"Couldn't read order. Error: {ex.ToString()}");
            }
            finally {
                res.StatusCode = (int)HttpStatusCode.OK;
                res.AsText("all good");
            }

        }, "POST");
        return Task.CompletedTask;
    }

    private async Task<bool> SendOrder(ShoppyOrderResponse order, DiscordGuild Client) {
        try {
            if (order.DiscordID.CalculateLength() != 18) return false;
            var user = await Client.GetMemberAsync(order.DiscordID);
            if (user == default) return false;
            await user.SendMessageAsync(embed: DiscordHelper.CreateEmbed("OrderHelper", order.ToString()));
            await user.SendFileAsync("account.txt", string.Join('\n', order.Products).AsStream());
            return true;
        } catch (Exception) {
            //DSharpPlus.Exceptions.NotFoundException
            return false;
        }
    }

    public void Stop() {
        Token.Cancel();
        logger.Debug("Webhook stopped");
    }


}

