import { useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Plus, Search, Edit2, Trash2, Users, Tag } from "lucide-react";
import { RoomForm } from "./room-form";

interface Room {
  id: string;
  roomNumber: string;
  building: string;
  capacity: number;
  type: string;
}

interface RoomType {
  id: string;
  name: string;
  description: string;
  defaultCapacity: number;
}

export default function RoomManagement() {
  const [rooms, setRooms] = useState<Room[]>([
    {
      id: "1",
      roomNumber: "101",
      building: "Science Building",
      capacity: 30,
      type: "Lecture Hall",
    },
    {
      id: "2",
      roomNumber: "102",
      building: "Science Building",
      capacity: 50,
      type: "Lecture Hall",
    },
    {
      id: "3",
      roomNumber: "201",
      building: "Engineering Building",
      capacity: 25,
      type: "Lab",
    },
    {
      id: "4",
      roomNumber: "301",
      building: "Arts Building",
      capacity: 40,
      type: "Seminar Room",
    },
    {
      id: "5",
      roomNumber: "501",
      building: "Main Building",
      capacity: 200,
      type: "Auditorium",
    },
    {
      id: "6",
      roomNumber: "202",
      building: "Engineering Building",
      capacity: 20,
      type: "Tutorial Room",
    },
  ]);

  const [roomTypes, setRoomTypes] = useState<RoomType[]>([
    {
      id: "1",
      name: "Lecture Hall",
      description: "Large classroom for lectures",
      defaultCapacity: 50,
    },
    {
      id: "2",
      name: "Lab",
      description: "Equipment and experiment space",
      defaultCapacity: 25,
    },
    {
      id: "3",
      name: "Seminar Room",
      description: "Interactive discussion space",
      defaultCapacity: 30,
    },
    {
      id: "4",
      name: "Tutorial Room",
      description: "Small group study room",
      defaultCapacity: 15,
    },
    {
      id: "5",
      name: "Auditorium",
      description: "Large assembly hall",
      defaultCapacity: 200,
    },
  ]);

  const [showForm, setShowForm] = useState(false);
  const [showTypeForm, setShowTypeForm] = useState(false);
  const [search, setSearch] = useState("");
  const [filterType, setFilterType] = useState<string | null>(null);

  const [typeFormData, setTypeFormData] = useState<Partial<RoomType>>({
    name: "",
    description: "",
    defaultCapacity: 30,
  });
  const [editingTypeId, setEditingTypeId] = useState<string | null>(null);

  const roomTypeNames = roomTypes.map((rt) => rt.name);

  const filteredRooms = rooms.filter((room) => {
    const matchesSearch =
      room.roomNumber.toLowerCase().includes(search.toLowerCase()) ||
      room.building.toLowerCase().includes(search.toLowerCase());
    const matchesType = filterType ? room.type === filterType : true;
    return matchesSearch && matchesType;
  });

  const getRoomTypeColor = (type: string) => {
    const colors: Record<string, string> = {
      "Lecture Hall": "bg-blue-500/10 text-blue-600",
      Lab: "bg-purple-500/10 text-purple-600",
      "Seminar Room": "bg-teal-500/10 text-teal-600",
      "Tutorial Room": "bg-orange-500/10 text-orange-600",
      Auditorium: "bg-pink-500/10 text-pink-600",
    };
    return colors[type] || "bg-gray-500/10 text-gray-600";
  };

  const handleSaveRoomType = () => {
    if (!typeFormData.name || !typeFormData.description) return;

    if (editingTypeId) {
      setRoomTypes(
        roomTypes.map((rt) =>
          rt.id === editingTypeId ? { ...rt, ...typeFormData } : rt
        ) as RoomType[]
      );
      setEditingTypeId(null);
    } else {
      setRoomTypes([
        ...roomTypes,
        {
          id: Date.now().toString(),
          ...typeFormData,
        } as RoomType,
      ]);
    }
    setTypeFormData({ name: "", description: "", defaultCapacity: 30 });
    setShowTypeForm(false);
  };

  const handleDeleteRoomType = (id: string) => {
    setRoomTypes(roomTypes.filter((rt) => rt.id !== id));
  };

  const handleEditRoomType = (roomType: RoomType) => {
    setEditingTypeId(roomType.id);
    setTypeFormData(roomType);
    setShowTypeForm(true);
  };

  return (
    <main>
      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">
            Room Management
          </h1>
          <p className="text-muted-foreground">
            Manage classroom and facility resources
          </p>
        </div>

        <Tabs defaultValue="rooms" className="space-y-4">
          <TabsList>
            <TabsTrigger value="rooms">Rooms</TabsTrigger>
            <TabsTrigger value="room-types">Room Types</TabsTrigger>
          </TabsList>

          <TabsContent value="rooms" className="space-y-4">
            <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
              <div className="relative flex-1 max-w-md">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 size-4 text-muted-foreground" />
                <Input
                  placeholder="Search rooms..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  className="pl-10"
                />
              </div>
              <Button
                onClick={() => setShowForm(true)}
                className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90"
              >
                <Plus className="size-4" />
                Add Room
              </Button>
            </div>

            {/* Room Type Filter */}
            <div className="flex flex-wrap gap-2">
              <Button
                variant={filterType === null ? "default" : "outline"}
                size="sm"
                onClick={() => setFilterType(null)}
              >
                All Types
              </Button>
              {roomTypeNames.map((type) => (
                <Button
                  key={type}
                  variant={filterType === type ? "default" : "outline"}
                  size="sm"
                  onClick={() => setFilterType(type)}
                >
                  {type}
                </Button>
              ))}
            </div>

            {showForm && (
              <RoomForm
                onClose={() => setShowForm(false)}
                onSave={(room) => {
                  setRooms([...rooms, room]);
                  setShowForm(false);
                }}
                roomTypes={roomTypeNames}
              />
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {filteredRooms.map((room) => (
                <Card
                  key={room.id}
                  className="hover:border-accent transition-colors"
                >
                  <CardContent className="pt-6">
                    <div className="flex flex-col gap-4 h-full">
                      <div>
                        <div className="flex items-start justify-between mb-2">
                          <h3 className="font-bold text-2xl text-accent">
                            Room {room.roomNumber}
                          </h3>
                          <span
                            className={`text-xs font-semibold px-2 py-1 rounded ${getRoomTypeColor(room.type)}`}
                          >
                            {room.type}
                          </span>
                        </div>
                        <p className="text-sm text-muted-foreground">
                          {room.building}
                        </p>
                      </div>

                      <div className="grid grid-cols-2 gap-4 py-4 border-y border-border">
                        <div>
                          <div className="flex items-center gap-2 mb-1">
                            <Users className="size-4 text-muted-foreground" />
                            <p className="text-xs text-muted-foreground">
                              Capacity
                            </p>
                          </div>
                          <p className="font-bold text-foreground">
                            {room.capacity}
                          </p>
                        </div>
                        <div>
                          <div className="flex items-center gap-2 mb-1">
                            <Tag className="size-4 text-muted-foreground" />
                            <p className="text-xs text-muted-foreground">
                              Status
                            </p>
                          </div>
                          <p className="font-bold text-green-600">Available</p>
                        </div>
                      </div>

                      <div className="flex gap-2 mt-auto">
                        <Button variant="ghost" size="sm" className="flex-1">
                          <Edit2 className="size-4 mr-2" />
                          Edit
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-destructive flex-1"
                        >
                          <Trash2 className="size-4 mr-2" />
                          Delete
                        </Button>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>

            {filteredRooms.length === 0 && (
              <Card className="text-center py-12">
                <CardContent>
                  <p className="text-muted-foreground">
                    No rooms found matching your search.
                  </p>
                </CardContent>
              </Card>
            )}
          </TabsContent>

          <TabsContent value="room-types" className="space-y-4">
            <div className="flex justify-end">
              <Button
                onClick={() => {
                  setEditingTypeId(null);
                  setTypeFormData({
                    name: "",
                    description: "",
                    defaultCapacity: 30,
                  });
                  setShowTypeForm(true);
                }}
                className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90"
              >
                <Plus className="size-4" />
                Add Room Type
              </Button>
            </div>

            {showTypeForm && (
              <Card className="border-2 border-accent">
                <CardContent className="pt-6">
                  <form className="space-y-4">
                    <div>
                      <label className="text-sm font-medium text-foreground block mb-1">
                        Type Name
                      </label>
                      <Input
                        placeholder="e.g., Lecture Hall"
                        value={typeFormData.name || ""}
                        onChange={(e) =>
                          setTypeFormData({
                            ...typeFormData,
                            name: e.target.value,
                          })
                        }
                      />
                    </div>
                    <div>
                      <label className="text-sm font-medium text-foreground block mb-1">
                        Description
                      </label>
                      <Input
                        placeholder="e.g., Large classroom for lectures"
                        value={typeFormData.description || ""}
                        onChange={(e) =>
                          setTypeFormData({
                            ...typeFormData,
                            description: e.target.value,
                          })
                        }
                      />
                    </div>
                    <div>
                      <label className="text-sm font-medium text-foreground block mb-1">
                        Default Capacity
                      </label>
                      <Input
                        type="number"
                        min="1"
                        max="500"
                        value={typeFormData.defaultCapacity || 30}
                        onChange={(e) =>
                          setTypeFormData({
                            ...typeFormData,
                            defaultCapacity: Number.parseInt(e.target.value),
                          })
                        }
                      />
                    </div>
                    <div className="flex gap-2 justify-end pt-4">
                      <Button
                        variant="outline"
                        type="button"
                        onClick={() => {
                          setShowTypeForm(false);
                          setEditingTypeId(null);
                        }}
                      >
                        Cancel
                      </Button>
                      <Button
                        type="button"
                        onClick={handleSaveRoomType}
                        className="bg-primary text-primary-foreground hover:bg-primary/90"
                      >
                        {editingTypeId ? "Update" : "Save"} Room Type
                      </Button>
                    </div>
                  </form>
                </CardContent>
              </Card>
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {roomTypes.map((roomType) => (
                <Card
                  key={roomType.id}
                  className="hover:border-accent transition-colors"
                >
                  <CardContent className="pt-6">
                    <div className="flex flex-col gap-4 h-full">
                      <div>
                        <h3 className="font-bold text-xl text-accent mb-1">
                          {roomType.name}
                        </h3>
                        <p className="text-sm text-muted-foreground">
                          {roomType.description}
                        </p>
                      </div>

                      <div className="py-4 border-y border-border">
                        <div className="flex items-center gap-2 mb-1">
                          <Users className="size-4 text-muted-foreground" />
                          <p className="text-xs text-muted-foreground">
                            Default Capacity
                          </p>
                        </div>
                        <p className="font-bold text-foreground">
                          {roomType.defaultCapacity}
                        </p>
                      </div>

                      <div className="flex gap-2 mt-auto">
                        <Button
                          variant="ghost"
                          size="sm"
                          className="flex-1"
                          onClick={() => handleEditRoomType(roomType)}
                        >
                          <Edit2 className="size-4 mr-2" />
                          Edit
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-destructive flex-1"
                          onClick={() => handleDeleteRoomType(roomType.id)}
                        >
                          <Trash2 className="size-4 mr-2" />
                          Delete
                        </Button>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>

            {roomTypes.length === 0 && (
              <Card className="text-center py-12">
                <CardContent>
                  <p className="text-muted-foreground">
                    No room types found. Create one to get started.
                  </p>
                </CardContent>
              </Card>
            )}
          </TabsContent>
        </Tabs>
      </div>
    </main>
  );
}
