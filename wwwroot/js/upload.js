function upload(file) {
	
	if (!file || !file.type.match(/image.*/)) return;
	var fd = new FormData();
	fd.append("image", file);
	fd.append("title", "stepa123");
	fd.append("client_secret", "bda650e874b80220bab96aa22c383a5e4581cead");
	
	var xhr = new XMLHttpRequest();
	xhr.open("POST", "https://api.imgur.com/3/image");
	xhr.setRequestHeader('Authorization', 'Client-ID 1337b94d530e99b');
	//var reader = new FileReader();
	//reader.onload = function {
	//	var originalImg = new Image();
	//	originalImg.src = e.target.result;
	//	CompressImage(originalImg.src);
	//};
	//reader.readAsDataURL(file);
	xhr.onload = function () {
		console.log(JSON.parse(xhr.responseText).data.link);
		$("#Input_Avatar").val(JSON.parse(xhr.responseText).data.link);
		$("#Label").text(file.name);
		$("#cartinka").empty();
		$("#cartinka").append('<img class="img-fluid rounded-circle" style="width: 100%;height: 100%;object-fit: cover;object-position: 0 0;" alt="smaple image" src="' + JSON.parse(xhr.responseText).data.link + '" />');
	};
	xhr.send(fd);
}
