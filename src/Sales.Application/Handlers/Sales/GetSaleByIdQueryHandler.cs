using AutoMapper;
using MediatR;
using Sales.Application.Commands.Sales;
using Sales.Application.DTOs;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;

namespace Sales.Application.Handlers.Sales;

public class GetSaleByIdQueryHandler(ISaleRepository saleRepository, IMapper mapper)
    : IRequestHandler<GetSaleByIdQuery, SaleDto>
{
    private readonly ISaleRepository _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<SaleDto> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {

        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);


        if (sale == null)
        {
            throw new NotFoundException(nameof(Sale), request.SaleId);
        }

        var saleDto = _mapper.Map<SaleDto>(sale);

        return saleDto;
    }
}