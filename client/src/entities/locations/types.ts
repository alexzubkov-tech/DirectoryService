export type Location = {
    id: string;
    name: string;
    address: AddressDto;
    timeZone: string;
    createdAt: string;
    departments: Array<DepartmentInfoDto>;
};

export type AddressDto = {
    country: string;
    city: string;
    street: string;
    buildingNumber: string;
};

export type DepartmentInfoDto = {
    id: string;
    identificator: string;
};
