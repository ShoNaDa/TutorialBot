using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Abstractions;
using VkNet.Model.RequestParams;
using System;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;
using VkNet;
using System.Linq;
using System.Web;

namespace VkBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration _configuration;

        private readonly IVkApi _vkApi;

        public CallbackController(IVkApi vkApi, IConfiguration configuration)
        {
            _vkApi = vkApi;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Callback([FromBody] Updates updates)
        {
            var api = new VkApi();
            api.Authorize(new ApiAuthParams 
            { 
                AccessToken = "3ddd6ecaaa61e0f77442dc030453633c4332fcbe750e3e26efbc55d127faa1edf05eb9f0778f727835e59"
            });

            var msg = Message.FromJson(new VkResponse(updates.Object));

            KeyboardBuilder key = new KeyboardBuilder(isOneTime: false);
            AddButtonParams params_button = new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Label = "Типа кнопка",
                Color = KeyboardButtonColor.Positive
            };
            key.AddButton(params_button);

            /*MessagesSendParams params_msg = new MessagesSendParams
            {
                RandomId = new DateTime().Millisecond,
                PeerId = msg.PeerId.Value,
                Attachments = new List<MediaAttachment>
                {
                    photo.FirstOrDefault()
                },
                Keyboard = keyboard
            }; */


            // Тип события
            switch (updates.Type)
            {
                // Ключ-подтверждение
                case "confirmation":
                {
                    return Ok(_configuration["Config:Confirmation"]);
                }

                // Новое сообщение
                case "message_new":
                {
                        try
                        {
                            //информация о пользователе
                            var info_user = api.Users.Get(new long[] 
                            {
                                msg.PeerId.Value
                            }).FirstOrDefault();

                            //фото
                            var photos = api.Photo.Get(new PhotoGetParams
                            {
                                AlbumId = PhotoAlbumType.Profile,
                                Count = 1,
                                OwnerId = msg.PeerId.Value
                            });

                            MessageKeyboard keyboard = key.Build();

                            // Десериализация
                            _vkApi.Messages.Send(new MessagesSendParams
                            {
                                RandomId = new DateTime().Millisecond,
                                PeerId = msg.PeerId.Value,
                                Attachments = photos,
                                Message = $"{info_user.FirstName} лошара &#128518;",
                                Keyboard = keyboard
                            });

                            //_vkApi.Messages.Send(params_msg);
                            break;
                        }
                        catch (Exception e)
                        {
                            // Десериализация
                            _vkApi.Messages.Send(new MessagesSendParams
                            {
                                RandomId = new DateTime().Millisecond,
                                PeerId = msg.PeerId.Value,
                                Message = Convert.ToString(e),
                                //Keyboard = keyboard
                            });
                            break;
                        }
                }
            }
            return Ok("ok");
        }
    }
}
