(function ($) {
    'use strict';

    var initialPersonalizationTagCount = 3;

    var $campaignId = $('#campaign-id');
    var $toAddress = $('#to-address');
    var $submit = $('#js-submit');

    var personalizationTagTemplateHtml = $('#personalization-tag-template').html();
    var $jsPersonalizationTagsContainer = $('#js-personalization-tags-container');

    $(function () {
        // Populate initial personalization tags
        for (var i = 0; i < initialPersonalizationTagCount; i++) {
            $jsPersonalizationTagsContainer
                .append(personalizationTagTemplateHtml);
        }

        $('#js-add-personalization-tag').on('click', function () {
            $jsPersonalizationTagsContainer
                .append(personalizationTagTemplateHtml);

            // 'Add' personalization tag animation
            $jsPersonalizationTagsContainer
                .children()
                .last()
                .hide()
                .show('fast');
        });

        $(document).on('click', '.js-remove-personalization-tag', function () {
            $(this)
                .parents('.js-personalization-tag-row')
                .hide('fast', function () {
                    $(this).remove();
                });
        });

        $('#js-submit').on('click', function (evt) {
            $submit
                .removeClass('btn-primary btn-danger btn-success')
                .addClass('btn-info')
                .prop('disabled', true)
                .html('<span class="glyphicon glyphicon-hourclass" aria-hidden="true"></span> ' +
                    'Sending the email ...');

            var data = {
                CampaignId: $campaignId.val(),
                ToAddress: $toAddress.val(),
                PersonalizationTags: $jsPersonalizationTagsContainer
                    .find('.personalization-tag-item')
                    .filter(function (i, element) {
                        return $(element).find('input[name="tag-name"]').val();
                    })
                    .map(function (i, element) {
                        var $element = $(element);

                        return {
                            name: $element.find('input[name="tag-name"]').val(),
                            value: $element.find('input[name="tag-value"]').val()
                        };
                    })
                    .get()
            };

            $.ajax(window.location.href + 'send', {
                type: 'POST',
                contentType: 'application/json; charset=UTF-8',
                data: JSON.stringify(data)
            })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    $submit
                        .addClass('btn-danger')
                        .html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> ' +
                            jqXHR.responseText);
                })
                .done(function (data, textStatus, jqXHR) {
                    console.log('Success response:');
                    console.log(data);

                    $submit
                        .addClass('btn-success')
                        .html('<span class="glyphicon glyphicon-ok" aria-hidden="true"></span> ' +
                            'Message sent! ' +
                            '<strong>' + data.emailsSent + '</strong> sent &ndash; ' +
                            '<strong>' + data.recipientsReceived + '</strong> received');
                })
                .always(function () {
                    $submit
                        .prop('disabled', false)
                        .removeClass('btn-info');
                });
        });
    })
})(jQuery)