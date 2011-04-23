<?php

	$secret = "sesam aukene";
	$cachedir = "cache";

	if(count($_POST) > 0 && $_POST["key"] == $secret) {
	
		$rebuild = false;

		foreach($_POST as $key => $val) {
		
			if($key == "drivers" || $key == "standing" || $key == "sessions" || $key == "track" || $key == "cars" || $key == "classes") {
				
				if((int)$_POST["sessionid"] > 0 && (int)$_POST["subsessionid"] > 0) {
					$data = stripslashes($val);
					if($key == "standing")
						$filename = $cachedir ."/". $_POST["sessionid"] ."-". $_POST["subsessionid"]. "-". $_POST["sessionnum"] ."-". $key .'.json';
					else
						$filename = $cachedir ."/". $_POST["sessionid"] ."-". $_POST["subsessionid"] ."-". $key .'.json';
					if(!is_file($filename))
						$rebuild = true;
					$fp = fopen($filename, 'w+');
					fwrite($fp, $data, strlen($data));
					fclose($fp);
				}
				else {
					echo "Session ID error!";
				}
				break;
			}
		}
		
		if($rebuild)
			rebuild_list();
	}
	else if(strlen($_GET["refresh"]) > 0) {
		rebuild_list();
	}
	else if ($_POST["key"] != $secret)
		echo "Key error!";
	else
		echo "General error!";

	function rebuild_list() {
		global $cachedir;
		
		if ($handle = opendir($cachedir)) {
			while (false !== ($file = readdir($handle))) {
				if ($file != "." && $file != "..") {
					$path_parts = pathinfo($cachedir . "/". $file);
					if($path_parts['extension'] == "json") {
						$parts = explode('-', $path_parts['filename']);
						if(count($parts) == 4)
							$jsons[] = $parts;
					}
				}
			}
			closedir($handle);
		}

		$data = json_encode($jsons);
		$fp = fopen($cachedir ."/list.json", 'w+');
		fwrite($fp, $data, strlen($data));
		fclose($fp);
	}

?>
