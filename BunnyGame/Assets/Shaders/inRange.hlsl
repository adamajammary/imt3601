float inRange (float input, float lower, float upper){
	return (ceil(input - lower) - ceil(input - upper));
}