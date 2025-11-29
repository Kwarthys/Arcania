extends MeshInstance3D
@export var speed : float = 0.5

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	rotate(Vector3.UP, 0.5 * TAU * delta)
