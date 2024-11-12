$(document).ready(function () {

    //var tenantId = ViewBag.TenantId;
    var userEmail = '@User.FindFirstValue(ClaimTypes.Email) ?? User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value';
    function fetchImageUrl(tenantId) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: `/@tenantName/Information/Get`,
                method: "GET",
                data: { tenantid: tenantId },
                success: function (imageUrl) {

                    if (imageUrl) {
                        // Find the div with class 'tenantImage'
                        const tenantDiv = $('.tenantImage');
                        console.log(imageUrl);
                        if (tenantDiv.length > 0) {
                            // Remove any existing image with the same ID to avoid duplicates
                            tenantDiv.find('#logoImage').remove();

                            // Create a new img element
                            const img = $('<img>', {
                                id: 'logoImage',
                                src: imageUrl,
                                style: 'width: 95px; padding-left: 10px; opacity: 100%;'
                            });

                            // Append the new img element to the div
                            tenantDiv.append(img);

                            resolve(imageUrl);
                        } else {
                            console.error("No element found with class 'tenantImage'");
                            reject(new Error("No element found with class 'tenantImage'"));
                        }
                    } else {
                        console.error("ImageUrl is missing in the response");
                        reject(new Error("ImageUrl is missing in the response"));
                    }
                },
                error: function (xhr, status, error) {
                    console.error("Cannot find Image src", status, error);
                    reject(new Error(`Cannot find Image src: ${status} ${error}`));
                }
            });
        });
    }


    // Usage with async/await
    async function updateImage(tenantId) {
        try {
            await fetchImageUrl(tenantId);
        } catch (error) {
            console.error("Failed to update image", error);
        }
    }

    // Call the function
    updateImage(1);




    var handlerValue = $("#page").val();
    debugger
    //$(".page-name").text(handlerValue ? handlerValue.replace('.html', ' ') : '');

    debugger
    // if ($("#isUser").val() === "True") {
    //     $("#layoutSidenav_nav").remove();
    // }

});