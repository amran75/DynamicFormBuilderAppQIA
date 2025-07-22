$(document).ready(function () {
    let fieldIndex = 0;

    $("#add-field").click(function () {
        if (!$("#Title").valid()) {
            $("#title-validation-placeholder").removeClass("d-none");
            $("#Title").addClass('is-invalid');
            $(".form-title-section").css('animation', 'shake 0.5s');
            setTimeout(() => {
                $(".form-title-section").css('animation', '');
            }, 500);
            return;
        }

        $("#title-validation-placeholder").addClass("d-none");

        const newFieldHtml = `
            <div class="card mb-4 field-group p-3">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <div class="field-header">Label ${fieldIndex + 1}</div>
                        <button type="button" class="btn btn-sm btn-outline-danger remove-field">
                            <i class="fas fa-trash-alt"></i> Remove
                        </button>
                    </div>

                    <div class="form-group mb-3">
                        <label>Label Name</label>
                        <input name="Fields[${fieldIndex}].Label" class="form-control" placeholder="e.g., Select Country" />
                    </div>

                    <div class="form-group mb-3">
                        <label>Options</label>
                        <select name="Fields[${fieldIndex}].SelectedOption" class="form-control">
                            <option value="">Select Item</option>
                            <option value="Option1">Option 1</option>
                            <option value="Option2">Option 2</option>
                            <option value="Option3">Option 3</option>
                        </select>
                        <input type="hidden" name="Fields[${fieldIndex}].Options" value="Option1,Option2,Option3" />
                    </div>

                    <div class="form-check">
                        <input type="checkbox" name="Fields[${fieldIndex}].IsRequired" class="form-check-input" value="true" />
                        <input type="hidden" name="Fields[${fieldIndex}].IsRequired" value="false" />
                        <label class="form-check-label">
                            Required Field <span class="text-danger">*</span>
                        </label>
                    </div>
                </div>
            </div>`;

        $("#fields-container").append(newFieldHtml);
        fieldIndex++;
    });

    $(document).on("click", ".remove-field", function () {
        $(this).closest(".field-group").remove();
    });
});
