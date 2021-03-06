// <copyright file="SmeFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process sending a notification card to SME team-
    ///  whenever user submits a feedback through bot menu or from response card.
    /// </summary>
    public class SmeFeedbackCard
    {
        /// <summary>
        /// This method will construct the card for SME team which will have the
        /// feedback details given by the user.
        /// </summary>
        /// <param name="data">user activity payload</param>
        /// <param name="userDetails">User details.</param>
        /// <returns>Sme facing feedback notification card.</returns>
        public static Attachment GetCard(ShareFeedbackCardPayload data, TeamsChannelAccount userDetails)
        {
            // Constructing adaptive card that is sent to SME team.
            AdaptiveCard smeFeedbackCard = new AdaptiveCard("1.0")
            {
               Body = new List<AdaptiveElement>
               {
                   new AdaptiveTextBlock()
                   {
                       Text = Resource.SMEFeedbackHeaderText,
                       Weight = AdaptiveTextWeight.Bolder,
                       Size = AdaptiveTextSize.Medium,
                   },
                   new AdaptiveTextBlock()
                   {
                       Text = string.Format(Resource.FeedbackAlertText, userDetails.Name),
                       Wrap = true,
                   },
                   new AdaptiveTextBlock()
                   {
                       Text = Resource.RatingTitle,
                       Weight = AdaptiveTextWeight.Bolder,
                       Wrap = true,
                   },
                   new AdaptiveTextBlock()
                   {
                       Text = GetRatingDisplayText(data.Rating),
                       Spacing = AdaptiveSpacing.None,
                       Wrap = true,
                   },
               },
               Actions = new List<AdaptiveAction>
               {
                   new AdaptiveOpenUrlAction
                   {
                       Title = string.Format(Resource.ChatTextButton, userDetails.GivenName),
                       UrlString = $"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(userDetails.UserPrincipalName)}"
                   }
               }
            };

            // Description fact is available in the card only when user enters description text.
            if (!string.IsNullOrWhiteSpace(data.Description))
            {
                smeFeedbackCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = Resource.DescriptionText,
                    Weight = AdaptiveTextWeight.Bolder,
                    Wrap = true,
                });
                smeFeedbackCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = CardHelper.TruncateStringIfLonger(data.Description, CardHelper.DescriptionMaxDisplayLength),
                    Spacing = AdaptiveSpacing.None,
                    Wrap = true,
                });
            }

            // Question asked fact and view article show card is available when feedback is on QnA Maker response.
            if (!string.IsNullOrWhiteSpace(data.KnowledgeBaseAnswer) && !string.IsNullOrWhiteSpace(data.UserQuestion))
            {
                smeFeedbackCard.Body.Add(new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new AdaptiveFact()
                        {
                            Title = Resource.QuestionAskedFactTitle,
                            Value = data.UserQuestion,
                        },
                    }
                });
                smeFeedbackCard.Actions.AddRange(new List<AdaptiveAction>
                {
                    new AdaptiveShowCardAction
                    {
                        Title = Resource.ViewArticleButtonText,
                        Card = new AdaptiveCard("1.0")
                        {
                            Body = new List<AdaptiveElement>
                            {
                               new AdaptiveTextBlock
                               {
                                   Text = CardHelper.TruncateStringIfLonger(data.KnowledgeBaseAnswer, CardHelper.KnowledgeBaseAnswerMaxDisplayLength),
                                   Wrap = true
                               }
                            }
                        }
                    }
                });
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = smeFeedbackCard,
            };
        }

        // Return the display string for the given rating
        private static string GetRatingDisplayText(string rating)
        {
            if (!Enum.TryParse(rating, out FeedbackRating value))
            {
                throw new ArgumentException($"{rating} is not a valid rating value", nameof(rating));
            }

            return Resource.ResourceManager.GetString($"{rating}RatingText");
        }
    }
}