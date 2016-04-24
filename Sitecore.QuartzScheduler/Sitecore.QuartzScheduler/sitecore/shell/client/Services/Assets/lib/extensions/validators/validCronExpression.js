EntityService.utils.validator.add("validCronExpression", function (value, params) {
    var result = false;

    var triggerDetailService = app.GetTriggerDetailEntityService();

    result = triggerDetailService.IsValidCronExpression(value);

    return result;
});